using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace I2T.Imaginng
{
	/// <summary>
	/// Point struct that can be bindable
	/// </summary>
	public struct PointB : INotifyPropertyChanged
	{
		private int x;
		private int y;

		public int X
		{
			get { return x;}
			set
			{
				this.x = value;
				OnPropertyChanged();
			}
		}
		public int Y
		{
			get { return y; }
			set
			{
				this.y = value;
				OnPropertyChanged();
			}
		}

		public PointB(int x, int y): this()
		{
			this.x = x;
			this.y = y;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
