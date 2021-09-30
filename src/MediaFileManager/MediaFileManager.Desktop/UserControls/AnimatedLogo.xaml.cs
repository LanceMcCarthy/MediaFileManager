using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MediaFileManager.Desktop.UserControls
{
    public partial class AnimatedLogo : UserControl
    {
        public AnimatedLogo()
        {
            InitializeComponent();
            Loaded += AnimatedLogo_Loaded;
        }

        private void AnimatedLogo_Loaded(object sender, RoutedEventArgs e)
        {
            (Resources["GearsRotatingStoryboard"] as Storyboard)?.Begin();
        }
    }
}
