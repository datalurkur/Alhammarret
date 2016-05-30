#include "pch.h"
#include "CardRecognizer.h"

using namespace Recognizer;
using namespace Platform;
using namespace std;
using namespace cv;

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
	red = Scalar(0, 0, 255);
	green = Scalar(0, 255, 0);
	blue = Scalar(255, 0, 0);
	purple = Scalar(255, 0, 255);
}

void CardRecognizer::SetFrame(WriteableBitmap^ rawFrame)
{
	frameMat = BitmapToBGRMat(rawFrame);
}

bool CardRecognizer::FilterContours()
{
	try
	{
		return FilterContours_Internal(frameMat);
	}
	catch (cv::Exception e)
	{
		return false;
	}
}

bool CardRecognizer::FindCorners()
{
	// Indexes
	int i, j;
	list<LineSegment>::iterator itr;
	list<LineSegment>::iterator innerItr;

	cornersDebug = frameMat.clone();

	// Determine the convex hull that contains the given contours
	vector<vector<Point>> hull(1);
	convexHull(contourEdges, hull[0]);
	drawContours(cornersDebug, hull, 0, red, 3, 8, vector<Vec4i>(), 0, Point());

	// Convex hull is not a rectangle
	if (hull[0].size() < 4)
	{
		return false;
	}

	// Compile a list of the line segments that comprise the hull
	list<LineSegment> lines;
	for (i = 0; i < hull[0].size(); ++i)
	{
		lines.push_back(LineSegment(i, hull[0][i], hull[0][(i + 1) % hull[0].size()]));
	}

	// Combine any lines that share a point and are parallel within a certain margin
	for (itr = lines.begin(); itr != lines.end(); ++itr)
	{
		for (innerItr = next(itr); innerItr != lines.end(); ++innerItr)
		{
			if (itr->CanCombine(*innerItr))
			{
				itr->Combine(*innerItr);
				list<LineSegment>::iterator temp = innerItr;
				innerItr--;
				lines.erase(temp);
			}
		}
	}

	// Sort the lines and grab the longest 4
	lines.sort();
	LineSegment* segments[4];
	for (i = 0, itr = lines.begin(); i < 4 && itr != lines.end(); ++i, ++itr)
	{
		segments[i] = &(*itr);
		line(cornersDebug, itr->P0, itr->P1, green, 2);
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

		LineSegment* s0 = segments[i];
		LineSegment* s1 = segments[j];
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
		circle(cornersDebug, cardCorners[i], 25, purple, 3);

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
		circle(cornersDebug, cardCorners[i], 25, green, 2);
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

	Mat transform = Mat::zeros(frameMat.rows, frameMat.cols, frameMat.type());
	transform = getPerspectiveTransform(cardCorners, targetCorners);
	warpPerspective(frameMat, transformedCard, transform, Size(kCardWidth, kCardHeight));

	return true;
}

WriteableBitmap^ CardRecognizer::GetContourDebug() { return BGRMatToBitmap(contoursDebug); }

WriteableBitmap^ CardRecognizer::GetCornersDebug() { return BGRMatToBitmap(cornersDebug); }

WriteableBitmap^ CardRecognizer::GetTransformedCard() {	return BGRMatToBitmap(transformedCard); }

WriteableBitmap^ CardRecognizer::GetNameRegion()
{
	int width = kCardWidth - kNameRPadding - kNameLPadding;
	Rect roi(kNameLPadding, kNameVPadding, width, kNameHeight);
	Mat cropped = transformedCard(roi);
	return BGRMatToBitmap(cropped);
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
	int i, j;
	list<LineSegment>::iterator itr;

	Mat canny = FindEdges(src);

	vector<vector<Point>> contours;
	contours.clear();
	vector<Vec4i> hierarchy;
	hierarchy.clear();

	// Find contours
	findContours(canny, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0));

	contoursDebug = src.clone();

	// Locate contours that fall within a certain range of length
	contourEdges.clear();
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
			drawContours(contoursDebug, contours, i, blue, 4, 8, hierarchy, 0, Point());
		}
		else if (contourLength > maxContourLength)
		{
			drawContours(contoursDebug, contours, i, red, 4, 8, hierarchy, 0, Point());
		}
		else
		{
			for (j = 0; j < contours[i].size(); ++j)
			{
				contourEdges.push_back(contours[i][j]);
			}
			drawContours(contoursDebug, contours, i, green, 4, 8, hierarchy, 0, Point());
		}
	}

	// Not enough suitable contours found
	if (contourEdges.size() < 4)
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

byte* CardRecognizer::GetPointerToPixelData(IBuffer^ buffer)
{
	Platform::Object^ obj = buffer;
	Microsoft::WRL::ComPtr<IInspectable> insp(reinterpret_cast<IInspectable*>(obj));

	Microsoft::WRL::ComPtr<IBufferByteAccess> bufferByteAccess;
	insp.As(&bufferByteAccess);

	byte* pixels = nullptr;
	bufferByteAccess->Buffer(&pixels);
	return pixels;
}

Mat CardRecognizer::BitmapToBGRAMat(WriteableBitmap^ wb)
{
	byte* pPixels = GetPointerToPixelData(wb->PixelBuffer);
	int height = wb->PixelHeight;
	int width = wb->PixelWidth;

	Mat mat = Mat(height, width, CV_8UC4);
	memcpy(mat.data, pPixels, 4 * height * width);
	return mat;
}

Mat CardRecognizer::BitmapToBGRMat(WriteableBitmap^ wb)
{
	byte* pPixels = GetPointerToPixelData(wb->PixelBuffer);
	int height = wb->PixelHeight;
	int width = wb->PixelWidth;

	Mat mat = Mat(height, width, CV_8UC4);
	memcpy(mat.data, pPixels, 4 * height * width);
	cvtColor(mat, mat, CV_BGRA2BGR);
	return mat;
}

WriteableBitmap^ CardRecognizer::BGRAMatToBitmap(Mat inputMat)
{
	WriteableBitmap^ wbmp = ref new WriteableBitmap(inputMat.cols, inputMat.rows);
	IBuffer^ buffer = wbmp->PixelBuffer;
	unsigned char* dstPixels;
	ComPtr<IBufferByteAccess> pBufferByteAccess;
	ComPtr<IInspectable> pBuffer((IInspectable*)buffer);
	pBuffer.As(&pBufferByteAccess);
	pBufferByteAccess->Buffer(&dstPixels);
	memcpy(dstPixels, inputMat.data, inputMat.step.buf[1] * inputMat.cols * inputMat.rows);
	return wbmp;
}

WriteableBitmap^ CardRecognizer::BGRMatToBitmap(Mat inputMatRaw)
{
	Mat inputMat = Mat(inputMatRaw.rows, inputMatRaw.cols, CV_8UC4);
	cvtColor(inputMatRaw, inputMat, CV_BGR2BGRA);
	WriteableBitmap^ wbmp = ref new WriteableBitmap(inputMat.cols, inputMat.rows);
	IBuffer^ buffer = wbmp->PixelBuffer;
	unsigned char* dstPixels;
	ComPtr<IBufferByteAccess> pBufferByteAccess;
	ComPtr<IInspectable> pBuffer((IInspectable*)buffer);
	pBuffer.As(&pBufferByteAccess);
	pBufferByteAccess->Buffer(&dstPixels);
	memcpy(dstPixels, inputMat.data, inputMat.step.buf[1] * inputMat.cols * inputMat.rows);
	return wbmp;
}

WriteableBitmap^ CardRecognizer::AMatToBitmap(Mat inputMatRaw)
{
	Mat inputMat = Mat(inputMatRaw.rows, inputMatRaw.cols, CV_8UC4);
	cvtColor(inputMatRaw, inputMat, CV_GRAY2RGBA);
	WriteableBitmap^ wbmp = ref new WriteableBitmap(inputMat.cols, inputMat.rows);
	IBuffer^ buffer = wbmp->PixelBuffer;
	unsigned char* dstPixels;
	ComPtr<IBufferByteAccess> pBufferByteAccess;
	ComPtr<IInspectable> pBuffer((IInspectable*)buffer);
	pBuffer.As(&pBufferByteAccess);
	pBufferByteAccess->Buffer(&dstPixels);
	memcpy(dstPixels, inputMat.data, inputMat.step.buf[1] * inputMat.cols * inputMat.rows);
	return wbmp;
}

LineSegment::LineSegment(int order, cv::Point p0, cv::Point p1)
{
	Order = order;
	P0 = p0;
	P1 = p1;
	ComputeCached();
}

bool LineSegment::CanCombine(const LineSegment& other)
{
	// Do the line segments share a point?
	if (P0 != other.P0 && P0 != other.P1 && P1 != other.P0 && P1 != other.P1) { return false; }

	// Find the angle between the two lines
	double dot = (V.x * other.V.x) + (V.y * other.V.y);
	double angle = acos(dot / (Length * other.Length));

	if (angle > kAngleCollapse || (3.14159 - angle) > kAngleCollapse) { return false; }

	return true;
}

void LineSegment::Combine(const LineSegment& other)
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

void LineSegment::ComputeCached()
{
	Length = sqrt(pow(P0.x - P1.x, 2) + pow(P0.y - P1.y, 2));
	V = Point2d(P1.x - P0.x, P1.y - P0.y);
}