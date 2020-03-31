﻿using flight.View;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace flight
{
    /// <summary>
    /// Interaction logic for Joystick.xaml
    /// </summary>

    public partial class Joystick : UserControl
    {
        //private flightManager flightManager;

        /// <summary>Current Rudder</summary>
        public static readonly DependencyProperty RudderProperty =
            DependencyProperty.Register("Rudder", typeof(double), typeof(Joystick), null);

        /// <summary>Current Elevator</summary>
        public static readonly DependencyProperty ElevatorProperty =
            DependencyProperty.Register("Elevator", typeof(double), typeof(Joystick), null);

        /// <summary>How often should be raised StickMove event in degrees</summary>
        public static readonly DependencyProperty RudderStepProperty =
            DependencyProperty.Register("RudderStep", typeof(double), typeof(Joystick), new PropertyMetadata(1.0));

        /// <summary>How often should be raised StickMove event in Elevator units</summary>
        public static readonly DependencyProperty ElevatorStepProperty =
            DependencyProperty.Register("ElevatorStep", typeof(double), typeof(Joystick), new PropertyMetadata(1.0));

        /* Unstable - needs work */
        ///// <summary>Indicates whether the joystick knob resets its place after being released</summary>
        //public static readonly DependencyProperty ResetKnobAfterReleaseProperty =
        //    DependencyProperty.Register(nameof(ResetKnobAfterRelease), typeof(bool), typeof(VirtualJoystick), new PropertyMetadata(true));

        /// <summary>Current Rudder in degrees from 0 to 360</summary>
        public double Rudder
        {
            get { return Convert.ToDouble(GetValue(RudderProperty)); }
            set { SetValue(RudderProperty, value); }
        }

        /// <summary>current Elevator (or "power"), from 0 to 100</summary>
        public double Elevator
        {
            get 
            {
                Console.WriteLine("Elevator " + Convert.ToDouble(GetValue(ElevatorProperty)) + "\n");

                return Convert.ToDouble(GetValue(ElevatorProperty)); 
            }
            set { SetValue(ElevatorProperty, value); }
        }

        /// <summary>How often should be raised StickMove event in degrees</summary>
        public double RudderStep
        {
            get { return Convert.ToDouble(GetValue(RudderStepProperty)); }
            set
            {
                if (value < 1) value = 1; else if (value > 90) value = 90;
                SetValue(RudderStepProperty, Math.Round(value));
            }
        }

        /// <summary>How often should be raised StickMove event in Elevator units</summary>
        public double ElevatorStep
        {
            get { return Convert.ToDouble(GetValue(ElevatorStepProperty)); }
            set
            {
                if (value < 1) value = 1; else if (value > 50) value = 50;
                SetValue(ElevatorStepProperty, value);
            }
        }

        /// <summary>Indicates whether the joystick knob resets its place after being released</summary>
        //public bool ResetKnobAfterRelease
        //{
        //    get { return Convert.ToBoolean(GetValue(ResetKnobAfterReleaseProperty)); }
        //    set { SetValue(ResetKnobAfterReleaseProperty, value); }
        //}

        /// <summary>Delegate holding data for joystick state change</summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="args">Holds new values for Rudder and Elevator</param>
        public delegate void OnScreenJoystickEventHandler(Joystick sender, VirtualJoystickEventArgs args);


        /// <summary>Delegate for joystick events that hold no data</summary>
        /// <param name="sender">The object that fired the event</param>
        public delegate void EmptyJoystickEventHandler(Joystick sender);

        /// <summary>This event fires whenever the joystick moves</summary>
        public event OnScreenJoystickEventHandler Moved;

        /// <summary>This event fires once the joystick is released and its position is reset</summary>
        public event EmptyJoystickEventHandler Released;

        /// <summary>This event fires once the joystick is captured</summary>
        public event EmptyJoystickEventHandler Captured;

        private Point _startPos;
        private double _prevRudder, _prevElevator;
        private double canvasWidth, canvasHeight;
        private readonly Storyboard centerKnob;

        public Joystick()
        {
            InitializeComponent();
            Knob.MouseLeftButtonDown += Knob_MouseLeftButtonDown;
            Knob.MouseLeftButtonUp += Knob_MouseLeftButtonUp;
            Knob.MouseMove += Knob_MouseMove;
            //Released += sendArgs;
            centerKnob = Knob.Resources["CenterKnob"] as Storyboard;
            //this.flightManager = flightManager.Instance;
        }

        private void Knob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPos = e.GetPosition(Base);
            _prevRudder = _prevElevator = 0;
            canvasWidth = Base.ActualWidth - KnobBase.ActualWidth;
            canvasHeight = Base.ActualHeight - KnobBase.ActualHeight;
            Captured?.Invoke(this);
            Knob.CaptureMouse();
            centerKnob.Stop();
        }

        // normelize joystick values
        private double normelize(double inputLimit, double outputLimit, double arg)
        {
            return (arg / inputLimit) * outputLimit;
        }
        private void Knob_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Knob.IsMouseCaptured) return;
            Point newPos = e.GetPosition(Base);
            Point deltaPos = new Point(newPos.X - _startPos.X, newPos.Y - _startPos.Y);
            double distance = Math.Round(Math.Sqrt(deltaPos.X * deltaPos.X + deltaPos.Y * deltaPos.Y));

            if (distance >= canvasWidth / 2 || distance >= canvasHeight / 2) return;
            Rudder = this.normelize((canvasWidth / 2), 1, deltaPos.X);
            Elevator = this.normelize((canvasWidth / 2), 1, -deltaPos.Y);
            knobPosition.X = deltaPos.X;
            knobPosition.Y = deltaPos.Y;

            if (Moved == null || (!(Math.Abs(_prevRudder - Rudder) > RudderStep) && !(Math.Abs(_prevElevator - Elevator) > ElevatorStep)))
            {
                return;
            }
            //Console.WriteLine("rf");
            Moved?.Invoke(this, new VirtualJoystickEventArgs { Rudder = Rudder, Elevator = Elevator });
            _prevRudder = Rudder;
            _prevElevator = Elevator;
            
        }

       
        private void Knob_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Knob.ReleaseMouseCapture();
            centerKnob.Begin();
        }

        private void centerKnob_Completed(object sender, EventArgs e)
        {
            Released?.Invoke(this);
            Rudder = Elevator = _prevRudder = _prevElevator = 0;
        }

    }
}