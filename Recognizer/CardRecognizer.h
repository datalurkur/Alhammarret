#pragma once
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/highgui/highgui.hpp"

using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;

namespace Recognizer
{
	[Windows::Foundation::Metadata::WebHostHidden]
	public ref class CardRecognizer sealed
	{
	private:
		const int kCardWidth = 500;
		const int kCardHeight = 750;

		const int kNameHeight = 50;
		const int kNameVPadding = 40;
		const int kNameLPadding = 25;
		const int kNameRPadding = 100;

	public:
		CardRecognizer();

		void SetFrame(WriteableBitmap^ rawFrame);

		void SetCannyParams(int lower, int upper, int kernel, int blur);
		void SetContourBounds(int min, int max);
		void SetAreaBounds(int min, int max);
		void SetRotation(int amount);

		bool FilterContours();
		bool FindCorners();
		bool IsolateCard();

		WriteableBitmap^ GetContourDebug();
		WriteableBitmap^ GetCornersDebug();
		WriteableBitmap^ GetTransformedCard();
		WriteableBitmap^ GetNameRegion();

	private:
		bool FilterContours_Internal(cv::Mat src);
		cv::Mat FindEdges(cv::Mat src);

		byte* GetPointerToPixelData(IBuffer^ buffer);
		cv::Mat BitmapToBGRAMat(WriteableBitmap^ wb);
		cv::Mat BitmapToBGRMat(WriteableBitmap^ wb);
		WriteableBitmap^ BGRAMatToBitmap(cv::Mat inputMat);
		WriteableBitmap^ BGRMatToBitmap(cv::Mat inputMat);
		WriteableBitmap^ AMatToBitmap(cv::Mat inputMat);

	private:
		cv::Mat frameMat;
		cv::Mat contoursDebug;
		cv::Mat cornersDebug;
		cv::Mat transformedCard;

		std::vector<cv::Point> contourEdges;
		cv::Point2f cardCorners[4];
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
		cv::Scalar red;
		cv::Scalar green;
		cv::Scalar blue;
		cv::Scalar purple;
	};

	class LineSegment
	{
	private:
		// Lines within n radians of each other will be collapsed
		const double kAngleCollapse = 0.1;

	public:
		LineSegment(int order, cv::Point p0, cv::Point p1);
		bool CanCombine(const LineSegment& other);
		void Combine(const LineSegment& other);

		friend bool operator<(const LineSegment& l, const LineSegment& r)
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