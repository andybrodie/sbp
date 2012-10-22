using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Locima.SlidingBlock.ViewModel
{
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

        public ImageAreaChooserViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public double ImageXOffset
        {
            get { return _imageXOffset; }
            set
            {
                _imageXOffset = value;
                OnNotifyPropertyChanged("ImageXOffset");
            }
        }

        public double ImageYOffset
        {
            get { return _imageYOffset; }
            set
            {
                _imageYOffset = value;
                OnNotifyPropertyChanged("ImageYOffset");
            }
        }

        public double CropLeft
        {
            get { return _cropLeft; }
            set
            {
                _cropLeft = value;
                OnNotifyPropertyChanged("CropLeft");
            }
        }

        public double CropTop
        {
            get { return _cropTop; }
            set
            {
                _cropTop = value;
                OnNotifyPropertyChanged("CropTop");
            }
        }

        public double CropWidth
        {
            get { return _cropWidth; }
            set
            {
                _cropWidth = value;
                OnNotifyPropertyChanged("CropWidth");
            }
        }

        public double CropHeight
        {
            get { return _cropHeight; }
            set
            {
                _cropHeight = value;
                OnNotifyPropertyChanged("CropHeight");
            }
        }

        public double TotalWidth
        {
            get { return _totalWidth; }
            set
            {
                _totalWidth = value;
                OnNotifyPropertyChanged("TotalWidth");
            }
        }

        public double TotalHeight
        {
            get { return _totalHeight; }
            set
            {
                _totalHeight = value;
                OnNotifyPropertyChanged("TotalHeight");
            }
        }

        public double CropBottom
        {
            get { return Math.Abs(CropTop + CropHeight); }
        }

        public double CropRight
        {
            get { return Math.Abs(CropLeft + CropWidth); }
        }

        public double CropRightToRightEdge
        {
            get { return Math.Abs(TotalWidth - CropRight); }
        }

        public double CropBottomToBottomEdge
        {
            get { return Math.Abs(TotalHeight - CropBottom); }
        }

        public WriteableBitmap Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnNotifyPropertyChanged("Image");
            }
        }

        private double _zoomAmount;
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