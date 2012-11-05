using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Locima.SlidingBlock.ViewModel
{


    /// <summary>
    /// The MVVM view model for the <see cref="ImageAreaChooser"/> page.
    /// </summary>
    public class ImageAreaChooserViewModel : ViewModelBase
    {
        private double _cropHeight;
        private double _cropLeft;
        private double _cropTop;
        private double _cropWidth;
        private WriteableBitmap _image;
        private double _imageXOffset;
        private double _imageYOffset;
        private double _totalHeight;
        private double _totalWidth;


        /// <summary>
        /// Hooks our own <see cref="ViewModelBase.PropertyChanged"/> events with <see cref="OnPropertyChanged"/>
        /// </summary>
        public ImageAreaChooserViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }


        /// <summary>
        /// The X offset from the left side of the image that we start displaying the image from (as X increases the picture appears to scroll right)
        /// </summary>
        public double ImageXOffset
        {
            get { return _imageXOffset; }
            set
            {
                _imageXOffset = value;
                OnNotifyPropertyChanged("ImageXOffset");
            }
        }

        /// <summary>
        /// The Y offset from the top of the image that we start displaying the image from (as Y increases the picture appears to scroll down)
        /// </summary>
        public double ImageYOffset
        {
            get { return _imageYOffset; }
            set
            {
                _imageYOffset = value;
                OnNotifyPropertyChanged("ImageYOffset");
            }
        }

        /// <summary>
        /// The distance from the left hand side of the screen (pixel offset 0) to the left edge of the crop area, used fro the dark rectangle on the left of the crop area
        /// </summary>
        public double CropLeft
        {
            get { return _cropLeft; }
            set
            {
                _cropLeft = value;
                OnNotifyPropertyChanged("CropLeft");
            }
        }

        /// <summary>
        /// The distance from the top of the screen to the the top of the crop area, used for the dark rectangle to the top of the crop area
        /// </summary>
        public double CropTop
        {
            get { return _cropTop; }
            set
            {
                _cropTop = value;
                OnNotifyPropertyChanged("CropTop");
            }
        }


        /// <summary>
        /// The width of the crop area that image that will be cut out from the whole image
        /// </summary>
        public double CropWidth
        {
            get { return _cropWidth; }
            set
            {
                _cropWidth = value;
                OnNotifyPropertyChanged("CropWidth");
            }
        }


        /// <summary>
        /// The height of the image that will be cut out from the whole image
        /// </summary>
        public double CropHeight
        {
            get { return _cropHeight; }
            set
            {
                _cropHeight = value;
                OnNotifyPropertyChanged("CropHeight");
            }
        }



        /// <summary>
        /// The total width of the available canvas containing the image
        /// </summary>
        public double TotalWidth
        {
            get { return _totalWidth; }
            set
            {
                _totalWidth = value;
                OnNotifyPropertyChanged("TotalWidth");
            }
        }

        /// <summary>
        /// The total height of the available canvas containing the image
        /// </summary>
        public double TotalHeight
        {
            get { return _totalHeight; }
            set
            {
                _totalHeight = value;
                OnNotifyPropertyChanged("TotalHeight");
            }
        }


        /// <summary>
        /// The bottom of the image
        /// </summary>
        public double CropBottom
        {
            get { return Math.Abs(CropTop + CropHeight); }
        }

        /// <summary>
        /// The right hand edge of the image
        /// </summary>
        public double CropRight
        {
            get { return Math.Abs(CropLeft + CropWidth); }
        }


        /// <summary>
        /// The difference between the <see cref="TotalWidth"/> and the right and edge of the image (<see cref="CropRight"/>), used for the dark rectangle to the right of the crop area
        /// </summary>
        public double CropRightToRightEdge
        {
            get { return Math.Abs(TotalWidth - CropRight); }
        }

        /// <summary>
        /// The difference between the <see cref="TotalHeight"/> and the right and edge of the image (<see cref="CropBottom"/>), used for the dark rectangle underneath the crop area
        /// </summary>
        public double CropBottomToBottomEdge
        {
            get { return Math.Abs(TotalHeight - CropBottom); }
        }

        /// <summary>
        /// The whole image that we're selecting a portion of
        /// </summary>
        public WriteableBitmap Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnNotifyPropertyChanged("Image");
            }
        }


        /// <summary>
        /// Backing field for <see cref="ZoomAmount"/>
        /// </summary>
        private double _zoomAmount;

        /// <summary>
        /// The zoom level of the image
        /// </summary>
        public double ZoomAmount
        {
            get { return _zoomAmount; }
            set { _zoomAmount = value;
            OnNotifyPropertyChanged("ZoomAmount");}
        }

        /// <summary>
        ///   Raises <see cref="ViewModelBase.PropertyChanged" /> events for properties which are dependent on other properties within this viewmodel
        /// </summary>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "CropTop":
                    OnNotifyPropertyChanged("CropBottom");
                    break;
                case "CropHeight":
                    OnNotifyPropertyChanged("CropBottom");
                    break;
                case "CropLeft":
                    OnNotifyPropertyChanged("CropRight");
                    break;
                case "CropWidth":
                    OnNotifyPropertyChanged("CropRight");
                    break;
                case "TotalWidth":
                    OnNotifyPropertyChanged("CropRightToRightEdge");
                    break;
                case "CropRight":
                    OnNotifyPropertyChanged("CropRightToRightEdge");
                    break;
                case "TotalHeight":
                    OnNotifyPropertyChanged("CropBottomToBottomEdge");
                    break;
                case "CropBottom":
                    OnNotifyPropertyChanged("CropBottomToBottomEdge");
                    break;
            }
        }
    }
}