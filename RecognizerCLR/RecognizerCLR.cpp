#include "stdafx.h"
#include "RecognizerCLR.h"
#include <list>

using namespace Recognizer;
using namespace std;
using namespace cv;
using namespace System::Windows::Media;
using namespace System::Runtime::InteropServices;

CardRecognizer::CardRecognizer()
{
	cannyLower = 10;
	cannyUpper = 20;
	cannyKernel = 3;
	cannyBlur = 3;
	minContourLength = 1000;
	maxContourLength = 2000;
	minBoxArea = 1000;
	maxBoxArea = 10000;

	// Various drawing tools
	red = new Scalar(0, 0, 255);
	green = new Scalar(0, 255, 0);
	blue = new Scalar(255, 0, 0);
	purple = new Scalar(255, 0, 255);

	frameMat = new cv::Mat();
	cornersDebug = new cv::Mat();
	contoursDebug = new cv::Mat();
	transformedCard = new cv::Mat();

    textDecompDebug = new cv::Mat();
    outputTextDebug = new cv::Mat();

	contourEdges = new std::vector<cv::Point>();
	cardCorners = new cv::Point2f[4];

    captureDevice = 0;
    captureInterface = 0;

    // Create filter objects with the default classifiers
    filter1 = new Ptr<text::ERFilter>(text::createERFilterNM1(text::loadClassifierNM1("Assets\\trained_classifierNM1.xml"), 8, 0.00015f, 0.13f, 0.2f, true, 0.1f));
	filter2 = new Ptr<text::ERFilter>(text::createERFilterNM2(text::loadClassifierNM2("Assets\\trained_classifierNM2.xml"), 0.5));
}

CardRecognizer::~CardRecognizer()
{
	delete filter1;
	delete filter2;

	delete cardCorners;
	delete contourEdges;

    delete outputTextDebug;
    delete textDecompDebug;

	delete transformedCard;
	delete contoursDebug;
	delete cornersDebug;
	delete frameMat;

	delete red;
	delete green;
	delete blue;
	delete purple;
}

bool CardRecognizer::StartCapturing()
{
    captureDevice = new VideoCapture(captureInterface);
    if (captureDevice->isOpened()) { return true; }
    else
    {
        delete captureDevice;
        captureDevice = 0;
        return false;
    }
}

void CardRecognizer::ChangeCaptureInterface()
{
    StopCapturing();
    captureInterface += 1;
    // TODO - Figure out how many there are
    StartCapturing();
}

void CardRecognizer::StopCapturing()
{
    delete captureDevice;
    captureDevice = 0;
}

bool CardRecognizer::CaptureFrame()
{
    if (!captureDevice) { return false;  }
    *captureDevice >> *frameMat;
    return true;
}

bool CardRecognizer::FilterContours()
{
	try
	{
		return FilterContours_Internal(*frameMat);
	}
	catch (cv::Exception e)
	{
		return false;
	}
}

bool CardRecognizer::FindCorners()
{
	// Indexes
	unsigned int i, j;
	list<RLineSegment>::iterator itr;
	list<RLineSegment>::iterator innerItr;

	*cornersDebug = frameMat->clone();

	// Determine the convex hull that contains the given contours
	vector<vector<Point>> hull(1);
	convexHull(*contourEdges, hull[0]);
	drawContours(*cornersDebug, hull, 0, *red, 3, 8, vector<Vec4i>(), 0, Point());

	// Convex hull is not a rectangle
	if (hull[0].size() < 4)
	{
		return false;
	}

	// Compile a list of the line segments that comprise the hull
	list<RLineSegment> lines;
	for (i = 0; i < hull[0].size(); ++i)
	{
		lines.push_back(RLineSegment(i, hull[0][i], hull[0][(i + 1) % hull[0].size()]));
	}

	// Combine any lines that share a point and are parallel within a certain margin
	for (itr = lines.begin(); itr != lines.end(); ++itr)
	{
		for (innerItr = next(itr); innerItr != lines.end(); ++innerItr)
		{
			if (itr->CanCombine(*innerItr))
			{
				itr->Combine(*innerItr);
				list<RLineSegment>::iterator temp = innerItr;
				innerItr--;
				lines.erase(temp);
			}
		}
	}

	// Sort the lines and grab the longest 4
	lines.sort();
	RLineSegment* segments[4];
	for (i = 0, itr = lines.begin(); i < 4 && itr != lines.end(); ++i, ++itr)
	{
		segments[i] = &(*itr);
		line(*cornersDebug, itr->P0, itr->P1, *green, 2);
	}

	// Put the lines in counter-clockwise order (they may have been shuffled around during the length sort)
	bool swapped = true;
	while (swapped)
	{
		swapped = false;
		for (i = 0; i < 3; ++i)
		{
			if (segments[i]->Order > segments[i + 1]->Order)
			{
				swap(segments[i], segments[i + 1]);
				swapped = true;
			}
		}
	}

	// Find the corners formed by the line intersections, computing intermediate area along the way
	double area = 0;
	for (i = 0; i < 4; ++i)
	{
		j = (i + 1) % 4;

		RLineSegment* s0 = segments[i];
		RLineSegment* s1 = segments[j];
		double x1 = s0->P0.x;
		double x2 = s0->P1.x;
		double x3 = s1->P0.x;
		double x4 = s1->P1.x;
		double y1 = s0->P0.y;
		double y2 = s0->P1.y;
		double y3 = s1->P0.y;
		double y4 = s1->P1.y;
		double denom = (x1 - x2)*(y3 - y4) - (y1 - y2)*(x3 - x4);
		double iX = ((x1*y2 - y1*x2)*(x3 - x4) - (x1 - x2)*(x3*y4 - y3*x4)) / denom;
		double iY = ((x1*y2 - y1*x2)*(y3 - y4) - (y1 - y2)*(x3*y4 - y3*x4)) / denom;
		cardCorners[i] = Point((int)iX, (int)iY);
		circle(*cornersDebug, cardCorners[i], 25, *purple, 3);

		area += (cardCorners[i].x * cardCorners[j].y) - (cardCorners[i].y * cardCorners[j].x);
	}
	area = abs(area / 2.0);

	if (area < minBoxArea || area > maxBoxArea)
	{
		return false;
	}

	// Requirements met
	for (i = 0; i < 4; ++i)
	{
		circle(*cornersDebug, cardCorners[i], 25, *green, 2);
	}

	return true;
}

bool CardRecognizer::IsolateCard()
{
	// Use the corner closest the 0,0 pixel as the upper-left
	float closestDist = INFINITY;
	int closestOffset;
	for (int i = 0; i < 4; ++i)
	{
		float dist = pow(cardCorners[i].x, 2) + pow(cardCorners[i].y, 2);
		if (dist < closestDist)
		{
			closestDist = dist;
			closestOffset = i;
		}
	}

	// Use the corners to form a perspective projection
	Point2f targetCorners[4];
	targetCorners[(0 + rotation + closestOffset) % 4] = Point2f(0.0f, 0.0f);
	targetCorners[(1 + rotation + closestOffset) % 4] = Point2f((float)kCardWidth, 0.0f);
	targetCorners[(2 + rotation + closestOffset) % 4] = Point2f((float)kCardWidth, (float)kCardHeight);
	targetCorners[(3 + rotation + closestOffset) % 4] = Point2f(0.0f, (float)kCardHeight);

	Mat transform = Mat::zeros(frameMat->rows, frameMat->cols, frameMat->type());
	transform = getPerspectiveTransform(cardCorners, targetCorners);
	warpPerspective(*frameMat, *transformedCard, transform, Size(kCardWidth, kCardHeight));

	return true;
}

System::String^ CardRecognizer::RecognizeText()
{
    unsigned int i, j;

    // DETECT TEXT
    // Get the name region
    int width = kCardWidth - kNameRPadding - kNameLPadding;
    Rect roi(kNameLPadding, kNameVPadding, width, kNameHeight);
    Mat nameRegion = (*transformedCard)(roi);

    // Convert the name region to grayscale
    Mat grayImage;
    cvtColor(nameRegion, grayImage, COLOR_RGB2GRAY);

    vector<Mat> channels;
    channels.push_back(grayImage);
    channels.push_back(255 - grayImage);

    vector<vector<text::ERStat>> regions(channels.size());
    for (i = 0; i < channels.size(); ++i)
    {
        (*filter1)->run(channels[i], regions[i]);
        (*filter2)->run(channels[i], regions[i]);
    }

    *textDecompDebug = Mat::zeros(nameRegion.rows + 2, nameRegion.cols + 2, CV_8UC1);
    vector<Vec2i> tempGroup;
    for (i = 0; i < regions.size(); ++i)
    {
        for (j = 0; j < regions[i].size(); ++j)
        {
            tempGroup.push_back(Vec2i(i, j));
        }
        Mat tempMat = Mat::zeros(nameRegion.rows + 2, nameRegion.cols + 2, CV_8UC1);
        ErDraw(channels, regions, tempGroup, tempMat);
        if (i > 0)
        {
            tempMat = tempMat / 2;
        }
        *textDecompDebug = *textDecompDebug | tempMat;
        tempGroup.clear();
    }

    // Detect character groups
    vector<vector<Vec2i>> nmRegionGroups;
    vector<Rect> nmBoxes;
    erGrouping(nameRegion, channels, regions, nmRegionGroups, nmBoxes, text::erGrouping_Modes::ERGROUPING_ORIENTATION_HORIZ);

    // RECOGNIZE TEXT
    Ptr<text::OCRTesseract> ocr = text::OCRTesseract::create();

    string outputText;

    Mat detectionOutput;
    Mat groupSegmentationOutput = Mat::zeros(nameRegion.rows + 2, nameRegion.cols + 2, CV_8UC1);
    nameRegion.copyTo(*outputTextDebug);
    nameRegion.copyTo(detectionOutput);
    float imageScale  = 600.f / nameRegion.rows;
    float fontScale = (float)(2 - imageScale) / 1.4f;

    vector<string> detectedWords;
    for (i = 0; i < nmBoxes.size(); ++i)
    {
        rectangle(detectionOutput, nmBoxes[i].tl(), nmBoxes[i].br(), Scalar(0, 255, 255), 3);

        Mat groupMat = Mat::zeros(nameRegion.rows + 2, nameRegion.cols + 2, CV_8UC1);
        ErDraw(channels, regions, nmRegionGroups[i], groupMat);

        Mat groupSegmentation;
        groupMat.copyTo(groupSegmentation);
        groupMat(nmBoxes[i]).copyTo(groupMat);
        copyMakeBorder(groupMat, groupMat, 15, 15, 15, 15, BORDER_CONSTANT, Scalar(0));

        vector<Rect> boxes;
        vector<string> words;
        vector<float> confidences;
        ocr->run(groupMat, outputText, &boxes, &words, &confidences, text::OCR_LEVEL_WORD);

        outputText.erase(remove(outputText.begin(), outputText.end(), '\n'), outputText.end());
        if (outputText.size() < 3)
        {
            continue;
        }

        for (j = 0; j < boxes.size(); ++j)
        {
            boxes[j].x += nmBoxes[i].x - 15;
            boxes[j].y += nmBoxes[i].y - 15;

            if ((words[j].size() < 2) || (confidences[j] < 51) || ((words[j].size() == 2) && (words[j][0] == words[j][1])) || ((words[j].size() < 4) && (confidences[j] < 60)) || IsRepetitive(words[j]))
                continue;
            detectedWords.push_back(words[j]);
            rectangle(*outputTextDebug, boxes[j].tl(), boxes[j].br(), Scalar(255, 0, 255), 3);
            Size word_size = getTextSize(words[j], FONT_HERSHEY_SIMPLEX, (double)fontScale, (int)(3 * fontScale), NULL);
            rectangle(*outputTextDebug, boxes[j].tl() - Point(3, word_size.height + 3), boxes[j].tl() + Point(word_size.width, 0), Scalar(255, 0, 255), -1);
            putText(*outputTextDebug, words[j], boxes[j].tl() - Point(1, 1), FONT_HERSHEY_SIMPLEX, fontScale, Scalar(255, 255, 255), (int)(3 * fontScale));
            groupSegmentationOutput = groupSegmentationOutput | groupSegmentation;
        }
    }

    return gcnew System::String(outputText.c_str());
}

void CardRecognizer::SetIntermediateData(IntermediateMat mat, System::String^ path)
{
    IntPtr ptrToNative = Marshal::StringToHGlobalAnsi(path);
    char* cPath = static_cast<char*>(ptrToNative.ToPointer());
    switch (mat)
    {
    case IntermediateMat::Preview:
        *frameMat = imread(cPath);
        break;
    case IntermediateMat::Contour:
        *contoursDebug = imread(cPath);
        break;
    case IntermediateMat::Corners:
        *cornersDebug = imread(cPath);
        break;
    case IntermediateMat::Transformed:
        *transformedCard = imread(cPath);
        break;
    case IntermediateMat::TextDecomp:
        *textDecompDebug = imread(cPath);
        break;
    case IntermediateMat::TextOutput:
        *outputTextDebug = imread(cPath);
        break;
    }
    Marshal::FreeHGlobal(ptrToNative);
}

WriteableBitmap^ CardRecognizer::GetIntermediateMat(IntermediateMat mat)
{
    Mat* source;
    bool needsFree = false;
    switch (mat)
    {
    case IntermediateMat::Preview:
        source = frameMat;
        break;
    case IntermediateMat::Contour:
        source = contoursDebug;
        break;
    case IntermediateMat::Corners:
        source = cornersDebug;
        break;
    case IntermediateMat::Transformed:
        source = transformedCard;
        break;
    case IntermediateMat::NameRegion:
    {
        int width = kCardWidth - kNameRPadding - kNameLPadding;
        Rect roi(kNameLPadding, kNameVPadding, width, kNameHeight);
        source = new Mat((*transformedCard)(roi));
        needsFree = true;
        break;
    }
    case IntermediateMat::TextDecomp:
        source = textDecompDebug;
        break;
    case IntermediateMat::TextOutput:
        source = outputTextDebug;
        break;
    }

    WriteableBitmap^ ret = BGRMatToBitmap(*source);
    if (needsFree)
    {
        delete source;
    }
    return ret;
}

void CardRecognizer::SetCannyParams(int lower, int upper, int kernel, int blur)
{
	cannyLower = lower;
	cannyUpper = upper;
	cannyKernel = kernel;
	cannyBlur = blur;
}

void CardRecognizer::SetContourBounds(int min, int max)
{
	minContourLength = min;
	maxContourLength = max;
}

void CardRecognizer::SetAreaBounds(int min, int max)
{
	minBoxArea = min;
	maxBoxArea = max;
}

void CardRecognizer::SetRotation(int amount)
{
	rotation = amount;
}

bool CardRecognizer::FilterContours_Internal(Mat src)
{
	// Indexes
	unsigned int i, j;
	list<RLineSegment>::iterator itr;

	Mat canny = FindEdges(src);

	vector<vector<Point>> contours;
	contours.clear();
	vector<Vec4i> hierarchy;
	hierarchy.clear();

	// Find contours
	findContours(canny, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

	*contoursDebug = src.clone();

	// Locate contours that fall within a certain range of length
	contourEdges->clear();
	for (i = 0; i < contours.size(); i++)
	{
		if (contours[i].size() < 2) { continue; }
		double contourLength = 0;
		for (j = 0; j < contours[i].size(); ++j)
		{
			Point p0 = contours[i][j];
			Point p1 = contours[i][(j + 1) % contours[i].size()];
			double distance = sqrt(pow(p0.x - p1.x, 2) + pow(p0.y - p1.y, 2));
			contourLength += distance;
		}

		if (contourLength < minContourLength)
		{
			drawContours(*contoursDebug, contours, i, *blue, 4, 8, hierarchy, 0, Point());
		}
		else if (contourLength > maxContourLength)
		{
			drawContours(*contoursDebug, contours, i, *red, 4, 8, hierarchy, 0, Point());
		}
		else
		{
			for (j = 0; j < contours[i].size(); ++j)
			{
				contourEdges->push_back(contours[i][j]);
			}
			drawContours(*contoursDebug, contours, i, *green, 4, 8, hierarchy, 0, Point());
		}
	}

	// Not enough suitable contours found
	if (contourEdges->size() < 4)
	{
		return false;
	}

	return true;
}

Mat CardRecognizer::FindEdges(cv::Mat src)
{
	// Convert the image to grayscale
	Mat src_gray;
	cvtColor(src, src_gray, CV_BGR2GRAY);

	// Reduce noise with a kernel 3x3
	Mat detected_edges;
	blur(src_gray, detected_edges, Size(cannyBlur, cannyBlur));

	// Detect edges
	Canny(detected_edges, detected_edges, cannyLower, cannyUpper, cannyKernel);

	return detected_edges;
}

WriteableBitmap^ CardRecognizer::BGRMatToBitmap(Mat inputMat)
{
	WriteableBitmap^ wbmp = gcnew WriteableBitmap(inputMat.cols, inputMat.rows, 96.0, 96.0, PixelFormats::Bgr24, nullptr);
	wbmp->Lock();
    int inputStride = inputMat.step;
    int outputStride = wbmp->BackBufferStride;
	uchar* inputBuffer = inputMat.data;
	uchar* outputBuffer = (uchar*)(void*)wbmp->BackBuffer;
    for (int i = 0; i < inputMat.rows; ++i)
    {
		memcpy(&(outputBuffer[i * outputStride]), &(inputBuffer[i * inputStride]), inputMat.cols * 3);
    }
    wbmp->AddDirtyRect(System::Windows::Int32Rect(0, 0, inputMat.cols, inputMat.rows));
	wbmp->Unlock();
	return wbmp;
}

void CardRecognizer::ErDraw(vector<Mat> &channels, vector<vector<text::ERStat>>& regions, vector<Vec2i> group, Mat& segmentation)
{
    for (unsigned int i = 0; i < group.size(); ++i)
    {
        text::ERStat er = regions[group[i][0]][group[i][1]];
        if (er.parent != NULL) // deprecate the root region
        {
            int newMaskVal = 255;
            int flags = 4 + (newMaskVal << 8) + FLOODFILL_FIXED_RANGE + FLOODFILL_MASK_ONLY;
            floodFill(channels[group[i][0]], segmentation, Point(er.pixel%channels[group[i][0]].cols, er.pixel / channels[group[i][0]].cols),
                Scalar(255), 0, Scalar(er.level), Scalar(0), flags);
        }
    }
}

bool CardRecognizer::IsRepetitive(const string& s)
{
    int count = 0;
    for (int i = 0; i<(int)s.size(); i++)
    {
        if ((s[i] == 'i') ||
            (s[i] == 'l') ||
            (s[i] == 'I'))
            count++;
    }
    if (count >((int)s.size() + 1) / 2)
    {
        return true;
    }
    return false;
}

RLineSegment::RLineSegment(int order, cv::Point p0, cv::Point p1)
{
	Order = order;
	P0 = p0;
	P1 = p1;
	ComputeCached();
}

bool RLineSegment::CanCombine(const RLineSegment& other)
{
	// Do the line segments share a point?
	if (P0 != other.P0 && P0 != other.P1 && P1 != other.P0 && P1 != other.P1) { return false; }

	// Find the angle between the two lines
	double dot = (V.x * other.V.x) + (V.y * other.V.y);
	double angle = acos(dot / (Length * other.Length));

	if (angle > kAngleCollapse || (3.14159 - angle) > kAngleCollapse) { return false; }

	return true;
}

void RLineSegment::Combine(const RLineSegment& other)
{
	if (P0 == other.P0)
	{
		P0 = other.P1;
	}
	else if (P1 == other.P1)
	{
		P1 = other.P0;
	}
	else if (P0 == other.P1)
	{
		P0 = other.P0;
	}
	else
	{
		P1 = other.P1;
	}
	ComputeCached();
}

void RLineSegment::ComputeCached()
{
	Length = sqrt(pow(P0.x - P1.x, 2) + pow(P0.y - P1.y, 2));
	V = Point2d(P1.x - P0.x, P1.y - P0.y);
}