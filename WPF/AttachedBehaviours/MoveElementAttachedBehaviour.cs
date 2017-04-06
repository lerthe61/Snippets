using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace BlaBlaBla.AttachedBehaviours
{
    public static class MoveElementAttachedBehaviour
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(FrameworkElement),
                new UIPropertyMetadata(false, IsEnabledChanged));

        private static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached(
                "State",
                typeof(State),
                typeof(MoveElementAttachedBehaviour),
                new FrameworkPropertyMetadata(new State()));

        public static bool GetIsEnabled(FrameworkElement frameworkElement)
        {
            return (bool) frameworkElement.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(IsEnabledProperty, value);
        }

        private static State GetState(FrameworkElement frameworkElement)
        {
            return (State) frameworkElement.GetValue(StateProperty);
        }

        private static void SetState(FrameworkElement frameworkElement, State state)
        {
            frameworkElement.SetValue(StateProperty, state);
        }

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element == null) return;

            if (!(e.NewValue is bool)) return;

            if ((bool)e.NewValue == true)
            {
                MakeMovable(element);
            }
            else
            {
                UnmakeMovable(element);
            }
        }

        private static void UnmakeMovable(FrameworkElement element)
        {
            element.MouseLeftButtonDown -= ElementOnMouseLeftButtonDown;
            element.MouseLeftButtonUp -= ElementOnMouseLeftButtonUp;
            element.MouseMove -= ElementOnMouseMove;
        }

        private static void MakeMovable(FrameworkElement element)
        {
            element.MouseLeftButtonDown += ElementOnMouseLeftButtonDown;
            element.MouseLeftButtonUp += ElementOnMouseLeftButtonUp;
            element.MouseMove += ElementOnMouseMove;
        }

        private static void ElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var element = (FrameworkElement)sender;
            var state = GetState(element);
            if (state.InDrag)
            {
                var currentPoint = mouseEventArgs.GetPosition(null);

                state.TranslateTransform.X = currentPoint.X - state.AnchorPoint.X;
                state.TranslateTransform.Y = currentPoint.Y - state.AnchorPoint.Y;
                
                element.RenderTransform = state.TranslateTransform;
                state.AnchorPoint = currentPoint;
            }
        }

        private static void ElementOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var element = (FrameworkElement)sender;
            var state = GetState(element);
            if (state.InDrag)
            {
                element.ReleaseMouseCapture();
                state.InDrag = false;
                mouseButtonEventArgs.Handled = true;
            }
        }

        private static void ElementOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var element = (FrameworkElement)sender;
            var state = GetState(element);
            state.AnchorPoint = mouseButtonEventArgs.GetPosition(null);
            element.CaptureMouse();
            state.InDrag = true;
            mouseButtonEventArgs.Handled = true;
        }

        private class State
        {
            public State()
            {
                TranslateTransform = new TranslateTransform();
            }

            public Point AnchorPoint { get; set; }
            public bool InDrag { get; set; }
            public TranslateTransform TranslateTransform {get; set; }
        }
    }
}