using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit;
using System.Windows.Media.Animation;

namespace WpfApp1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	
	public partial class MainWindow : Window
	{
		private double _angle = 0;
		private double angle 
		{ 
			get { return _angle; } 
			set { _angle = value; Output.Content = value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture); }
		}
		private int stepsperrevolution = 400;
		private int shaftteeth = 15;
		private int tableteeth = 177;
		private int _destination = 0;
		private int destination 
		{ 
			get { return _destination; } 
			set { _destination = value; angle = (currentaction - (double)value) / (stepsperrevolution * ((double)tableteeth / shaftteeth)) * 360; }
		}
		private int currentaction = 0;
		private bool _rotating = false;
		private long totalsteps = 0;
		private bool rotating
		{
			get { return _rotating; }
			set { _rotating = value; if (value) statusLabel.Content = "Вращение"; else statusLabel.Content = "Ожидание"; }
		}
		private UDPSocket server = new UDPSocket();
		private UDPSocket client = new UDPSocket();

		public MainWindow()
		{
			server.Server(8888);
			client.Client("192.168.1.1", 8888);
			InitializeComponent();
			tab1.IsSelected = true;
			rad2.IsChecked = true;
			client.ReceivedData += HandleRecievedData;
		}

		private void HandleRecievedData(object sender, string data)
        {
			if (data[0] == 'F')
			{
				data = data.Trim('F');
				rotating = false;
			}
			destination = int.Parse(data);
        }

		private void Rotation(long steps)
        {
			rotating = true;
			totalsteps += steps;
			client.Send(steps.ToString());
			testLabel.Content = totalsteps.ToString();
		}

		private void ForceStop_Click(object sender, RoutedEventArgs e)
		{
			client.Send("S");
			rotating = false;
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btn1_Click(object sender, RoutedEventArgs e)
		{
			tab1.IsSelected = true;
			Style PressedStyle = FindResource("PressedButton") as Style;
			btn1.Style = PressedStyle;
			Style ReleasedStyle = FindResource("SimpleButton") as Style;
			btn2.Style = ReleasedStyle;
		}

		private void btn2_Click(object sender, RoutedEventArgs e)
		{
			tab2.IsSelected = true;
			Style PressedStyle = FindResource("PressedButton") as Style;
			btn2.Style = PressedStyle;
			Style ReleasedStyle = FindResource("SimpleButton") as Style;
			btn1.Style = ReleasedStyle;
		}

		private void rad1_Checked(object sender, RoutedEventArgs e)
		{
			angleBox.IsEnabled = true;
			minusAngle.IsEnabled = true;
			plusAngle.IsEnabled = true;
		}

		private void rad2_Checked(object sender, RoutedEventArgs e)
		{
			angleBox.IsEnabled = false;
			minusAngle.IsEnabled = false;
			plusAngle.IsEnabled = false;
		}

		private void rad3_Checked(object sender, RoutedEventArgs e)
		{
			angleBox.IsEnabled = false;
			minusAngle.IsEnabled = false;
			plusAngle.IsEnabled = false;
		}

		private void btnHome_Click(object sender, RoutedEventArgs e)
		{
			Rotation(0-totalsteps);
		}

		private void btnSetHome_Click(object sender, RoutedEventArgs e)
		{
			totalsteps = 0;
		}

		private void minusAngle_Click(object sender, RoutedEventArgs e)
		{
			double OutVal = double.Parse(angleBox.Text, System.Globalization.CultureInfo.InvariantCulture);
			OutVal -= 1;
			angleBox.Text = OutVal.ToString("00.0", System.Globalization.CultureInfo.InvariantCulture);
		}

		private void plusAngle_Click(object sender, RoutedEventArgs e)
		{
			double OutVal = double.Parse(angleBox.Text, System.Globalization.CultureInfo.InvariantCulture);
			OutVal += 1;
			angleBox.Text = OutVal.ToString("00.0", System.Globalization.CultureInfo.InvariantCulture);
		}

		private void angleBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9.]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void angleBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (angleBox.Text != String.Empty) angle = double.Parse(angleBox.Text, System.Globalization.CultureInfo.InvariantCulture);
		}

		private void minusSpeed_Click(object sender, RoutedEventArgs e)
		{
			double OutVal = double.Parse(speedBox.Text, System.Globalization.CultureInfo.InvariantCulture);
			OutVal -= 0.1;
			speedBox.Text = OutVal.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
		}

		private void plusSpeed_Click(object sender, RoutedEventArgs e)
		{
			double OutVal = double.Parse(speedBox.Text, System.Globalization.CultureInfo.InvariantCulture);
			OutVal += 0.1;
			speedBox.Text = OutVal.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
		}

		private void speedBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9.]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void speedBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (speedBox.Text != String.Empty)
				client.Send($"V{((int)(stepsperrevolution * (tableteeth / shaftteeth) * double.Parse(speedBox.Text, System.Globalization.CultureInfo.InvariantCulture) / 60)).ToString()}");
		}

		private void btn_Click(object sender, RoutedEventArgs e)
		{
			Label label = new Label();
			label.Content = "asd";
			Scroll.Children.Add(label);
		}

		private void ResumeRotation(bool resume)
        {
			if (!resume && rotating)
            {
				client.Send("S");
				rotating = false;
				currentaction = destination;
			}
            else if (resume && !rotating) Rotation(currentaction);	
		}

		private void PlayPause_Click(object sender, RoutedEventArgs e)
		{
			if (rotating) ResumeRotation(false);
			else ResumeRotation(true);
			/*
			if (rad1.IsChecked == true)
            {
				if (!rotating) Rotation((long)(stepsperrevolution * ((double)tableteeth / shaftteeth) * double.Parse(angleBox.Text, System.Globalization.CultureInfo.InvariantCulture) / 360));
				else
				{
					client.Send("S");
					rotating = false;
				}
			}
			else if (rad3.IsChecked == true)
            {
				if (!rotating) Rotation(32000);
				else
                {
					client.Send("S");
					rotating = false;
				}
			}
			*/
		}

		private void RotateCW_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (rad1.IsChecked == false) Rotation(32000);
			else if (!rotating) Rotation((long)(stepsperrevolution * ((double)tableteeth / shaftteeth) * double.Parse(angleBox.Text, System.Globalization.CultureInfo.InvariantCulture) / 360));
		}

		private void RotateCW_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (rad2.IsChecked == true) ForceStop_Click(new object(), new RoutedEventArgs());
		}

        private void RotateCCW_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			if (rad1.IsChecked == false) Rotation(-32000);
			else if (!rotating) Rotation(0-(long)(stepsperrevolution * ((double)tableteeth / shaftteeth) * double.Parse(angleBox.Text, System.Globalization.CultureInfo.InvariantCulture) / 360));
		}

        private void RotateCCW_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
			if (rad2.IsChecked == true) ForceStop_Click(new object(), new RoutedEventArgs());
		}
		private int count = 1;
		private void testbtnAdd_Click(object sender, RoutedEventArgs e)
		{
			count++;
			Label dyn = new Label();
			//dyn.Name = "dyn" + count;
			Scroll.Children.Add(dyn);
		}

		private void testbtnRemove_Click(object sender, RoutedEventArgs e)
		{
			Label lbl = (Label)this.Scroll.FindName("dyn" + count);
			this.Scroll.Children.Remove(lbl);
			if (count == 1) System.Windows.MessageBox.Show("a vse");
			else count--;
		}
	}
}
