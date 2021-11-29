using Lab5.Infrastructure.Commands;
using Lab5.ViewModels.Base;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Markup;
using System;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Lab5.ViewModels
{
	[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
	public class MainWindowViewModel : ViewModel
	{
		public MainWindowViewModel()
		{
			LoadImageCommand = new LambdaCommand(OnLoadImageCommandExecuted, CanLoadImageCommandExecute);
			BrowseImageCommand = new LambdaCommand(OnBrowseImageCommandExecuted, CanBrowseImageCommandExecute);
			GetNegativeImageCommand = new LambdaCommand(OnGetNegativeImageCommandExecuted, CanGetNegativeImageCommandExecute);
			BinarizeCommand = new LambdaCommand(OnBinarizeCommandExecuted, CanBinarizeCommandExecute);
			FiftyShadesOfGrayCommand = new LambdaCommand(OnFiftyShadesOfGrayExecuted, CanFiftyShadesOfGrayExecute);
			SetBrightnessCommand = new LambdaCommand(OnSetBrightnessCommandExecuted, CanSetBrightnessCommandExecute);
		}

		#region Properties
		private string _title = "Title";
		public string Title { get => _title; set => Set(ref _title, value); }

		private string _status = "Status";
		public string Status { get => _status; set => Set(ref _status, value); }

		private string _pathToImage = "";
		public string PathToImage { get => _pathToImage; set => Set(ref _pathToImage, value); }

		private int _brightnessOld;
		private int _brightness = 0;
		public int Brightness
		{ 
			get => _brightness;
			set
			{
				Set(ref _brightness, value);
				if (CanSetBrightnessCommandExecute(null))
					OnSetBrightnessCommandExecuted(null);
			}
		}

		private int _contrast = 0;
		public int Contrast { get => _contrast; set => Set(ref _contrast, value); }

		private WriteableBitmap _imageSourceOld;
		private WriteableBitmap _imageSource;
		public WriteableBitmap ImageSource { get => _imageSource; set => Set(ref _imageSource, value); }
		#endregion

		#region Commands
		#region LoadImageCommand
		public ICommand LoadImageCommand { get; }
		private void OnLoadImageCommandExecuted(object p)
		{
			try
			{
				ImageSource = new WriteableBitmap(new BitmapImage(new Uri(PathToImage)));
				_imageSourceOld = ImageSource.CloneCurrentValue();
			}
			catch (Exception e)
			{
				Status = e.Message;
			}
		}
		private bool CanLoadImageCommandExecute(object p) => !string.IsNullOrWhiteSpace(PathToImage);
		#endregion

		#region BrowseImageCommand
		public ICommand BrowseImageCommand { get; }
		private void OnBrowseImageCommandExecuted(object p)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = Environment.CurrentDirectory;
			dialog.Filter = "Картинки(*.JPG;*.GIF;*.BMP)|*.JPG;*.GIF;*.BMP";
			if (dialog.ShowDialog() == true)
			{
				PathToImage = dialog.FileName;
			}
		}
		private bool CanBrowseImageCommandExecute(object p) => true;
		#endregion

		#region GetNegativeImageCommand
		public ICommand GetNegativeImageCommand { get; }
		private void OnGetNegativeImageCommandExecuted(object p)
		{
			try
			{
				ProcessImage(ImageSource, NegativeImage);
				_imageSourceOld = ImageSource.CloneCurrentValue();
			}
			catch (Exception e)
			{
				Status = e.Message;
			}
		}
		private bool CanGetNegativeImageCommandExecute(object p) => ImageSource != null;
		#endregion

		#region BinarizeCommand
		public ICommand BinarizeCommand { get; }
		private void OnBinarizeCommandExecuted(object p)
		{
			try
			{
				ProcessImage(ImageSource, Binarizate);
				_imageSourceOld = ImageSource.CloneCurrentValue();
			}
			catch (Exception e)
			{
				Status = e.Message;
			}
		}
		private bool CanBinarizeCommandExecute(object p) => ImageSource != null;
		#endregion

		#region FiftyShadesOfGray
		public ICommand FiftyShadesOfGrayCommand { get; }
		private void OnFiftyShadesOfGrayExecuted(object p)
		{
			try
			{
				ProcessImage(ImageSource, FiftyShadesOfGray);
				_imageSourceOld = ImageSource.CloneCurrentValue();
			}
			catch (Exception e)
			{
				Status = e.Message;
			}
		}
		private bool CanFiftyShadesOfGrayExecute(object p) => ImageSource != null;
		#endregion

		#region SetBrightnessCommand
		public ICommand SetBrightnessCommand { get; }
		private void OnSetBrightnessCommandExecuted(object p)
		{
			try
			{
				ImageSource = _imageSourceOld.CloneCurrentValue();
				ProcessImage(ImageSource, SetBrightness);
			}
			catch (Exception e)
			{
				Status = e.Message;
			}
		}
		private bool CanSetBrightnessCommandExecute(object p) => ImageSource != null;
		#endregion
		#endregion

		private void ProcessImage(WriteableBitmap bitmap, Action<byte[]> action)
		{
			int width = bitmap.PixelWidth;
			int height = bitmap.PixelHeight;
			int pixelsAmount = bitmap.BackBufferStride * height;
			byte[] bytes = new byte[pixelsAmount];
			bitmap.CopyPixels(bytes, bitmap.BackBufferStride, 0);
			action.Invoke(bytes);
			Int32Rect rect = new Int32Rect(0, 0, width, height);
			bitmap.WritePixels(rect, bytes, bitmap.BackBufferStride, 0);
		}

		private void NegativeImage(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i += 4)
			{
				bytes[i] = (byte)(255 - bytes[i]);  // b
				bytes[i + 1] = (byte)(255 - bytes[i + 1]);  // g
				bytes[i + 2] = (byte)(255 - bytes[i + 2]);  // r
			}
		}

		private void Binarizate(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i += 4)
			{
				if ((bytes[i] + bytes[i + 1] + bytes[i + 2]) < 360)
				{
					bytes[i] = 0;
					bytes[i + 1] = 0;
					bytes[i + 2] = 0;
				}
				else
				{
					bytes[i] = 255;
					bytes[i + 1] = 255;
					bytes[i + 2] = 255;
				}
			}
		}

		private void FiftyShadesOfGray(byte[] bytes)
		{
			byte Y;
			for (int i = 0; i < bytes.Length; i += 4)
			{
				Y = (byte)(bytes[i] * 0.114 + 0.5876 * bytes[i + 1] + 0.299 * bytes[i + 2]);
				bytes[i] = Y;
				bytes[i + 1] = Y;
				bytes[i + 2] = Y;
			}
		}

		private void SetBrightness(byte[] bytes)
		{
			int value;
			for (int i = 0; i < bytes.Length; i += 4)
			{
				value = bytes[i];
				value += (int)(_brightness);
				if (value > 255) value = 255;
				if (value < 0) value = 0;
				bytes[i] = (byte)value;

				value = bytes[i + 1];
				value += (int)(_brightness);
				if (value > 255) value = 255;
				if (value < 0) value = 0;
				bytes[i + 1] = (byte)value;

				value = bytes[i + 2];
				value += (int)(_brightness);
				if (value > 255) value = 255;
				if (value < 0) value = 0;
				bytes[i + 2] = (byte)value;
			}
		}

		private int CalculateBrightness(WriteableBitmap bitmap)
		{
			int width = bitmap.PixelWidth;
			int height = bitmap.PixelHeight;
			int pixelsAmount = bitmap.BackBufferStride * height;
			byte[] bytes = new byte[pixelsAmount];
			bitmap.CopyPixels(bytes, bitmap.BackBufferStride, 0);
			double Y = 0;
			for (int i = 0; i < bytes.Length; i += 4)
			{
				Y += (bytes[i] * 0.114 + 0.5876 * bytes[i + 1] + 0.299 * bytes[i + 2]) / bytes.Length;
			}
			return Y > 255 ? 255 : (int)Y;
		}
	}
}
