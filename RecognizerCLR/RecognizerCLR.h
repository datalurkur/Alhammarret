// RecognizerCLR.h

#pragma once

#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/text.hpp"

using namespace System;
using namespace System::Windows::Media::Imaging;
using namespace std;

namespace Recognizer
{
	public ref class CardRecognizer sealed
	{
	private:
		const int kCardWidth = 1000;
		const int kCardHeight = 1500;

		const int kNameHeight = 200;
		const int kNameVPadding = 80;
		const int kNameLPadding = 50;
		const int kNameRPadding = 200;

	public:
		CardRecognizer();
		virtual ~CardRecognizer();

        bool StartCapturing();
        void ChangeCaptureInterface();
        void StopCapturing();
        bool CaptureFrame();

		void SetCannyParams(int lower, int upper, int kernel, int blur);
		void SetContourBounds(int min, int max);
		void SetAreaBounds(int min, int max);
		void SetRotation(int amount);

		bool FilterContours();
		bool FindCorners();
		bool IsolateCard();
        System::String^ RecognizeText();

        WriteableBitmap^ GetPreview();
		WriteableBitmap^ GetContourDebug();
		WriteableBitmap^ GetCornersDebug();
		WriteableBitmap^ GetTransformedCard();
		WriteableBitmap^ GetNameRegion();

	private:
		bool FilterContours_Internal(cv::Mat src);
		cv::Mat FindEdges(cv::Mat src);

		WriteableBitmap^ BGRMatToBitmap(cv::Mat inputMat);

        void ErDraw(vector<cv::Mat> &channels, vector<vector<cv::text::ERStat>>& regions, vector<cv::Vec2i> group, cv::Mat& segmentation);
        bool IsRepetitive(const string& s);

	private:
        cv::VideoCapture* captureDevice;
        int captureInterface;

		cv::Mat* frameMat;
		cv::Mat* contoursDebug;
		cv::Mat* cornersDebug;
		cv::Mat* transformedCard;

		std::vector<cv::Point>* contourEdges;
		cv::Point2f* cardCorners;
		int cannyLower;
		int cannyUpper;
		int cannyKernel;
		int cannyBlur;
		int minContourLength;
		int maxContourLength;
		int minBoxArea;
		int maxBoxArea;
		int rotation;

		// Various drawing tools
		cv::Scalar* red;
		cv::Scalar* green;
		cv::Scalar* blue;
		cv::Scalar* purple;
	};

	class RLineSegment
	{
	private:
		// Lines within n radians of each other will be collapsed
		const double kAngleCollapse = 0.1;

	public:
		RLineSegment(int order, cv::Point p0, cv::Point p1);
		bool CanCombine(const RLineSegment& other);
		void Combine(const RLineSegment& other);

		friend bool operator<(const RLineSegment& l, const RLineSegment& r)
		{
			// We want to sort line segments in descending order
			return l.Length > r.Length;
		}

	private:
		void ComputeCached();

	public:
		int Order;
		cv::Point P0;
		cv::Point P1;
		cv::Point2d V;
		double Length;
	};
}