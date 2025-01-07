using System;
using System.Device.Gpio;
using System.Threading.Tasks;

namespace FourOneShot.Pi.Devices
{
    /// <summary>
    /// Doya DD2702H 15-channel roller blind RF remote control.
    /// Remote control unit driven by GPIO pins, via relays/opto-couplers/transistors.
    /// Only one instance of this class should be active on the system at any time.
    /// Not thread-safe. Side-effects are expected if methods are not called sequentially.
    /// GPIO pin mappings are defined in <see cref="BlindRemotePins"/>.
    /// </summary>
    public class DD2702H : IDisposable
    {
        protected const int MinChannel = 0;
        protected const int MaxChannel = 15;
        protected const int ButtonPressMilliseconds = 240;
        protected const int SwitchDebounceMilliseconds = 120;
        protected const int BrownOutWaitMilliseconds = 1500;
        protected const int StartUpWaitMilliseconds = 2500;
        protected const int PairingMilliseconds = 4000;
        protected const int SleepTimeOutMilliseconds = 10000;

        protected readonly GpioController _controller;

        protected bool _disposed = false;
        protected int _channel = 1;
        protected DateTime? _lastButtonPressTime = null;


        /// <summary>
        /// The currently selected channel on the remote.
        /// </summary>
        public virtual int Channel
        {
            get { return _channel; }
        }

        /// <summary>
        /// Initialises a new instance with a real or stubbed/emulated GPIO controller.
        /// </summary>
        /// <param name="controller"></param>
        public DD2702H(GpioController controller)
        {
            _controller = controller;

            // Set-up the required GPIO pins to output mode.
            _controller.OpenPin((int)BlindRemotePins.Power, PinMode.Output);
            _controller.OpenPin((int)BlindRemotePins.Open, PinMode.Output);
            _controller.OpenPin((int)BlindRemotePins.Close, PinMode.Output);
            _controller.OpenPin((int)BlindRemotePins.Stop, PinMode.Output);
            _controller.OpenPin((int)BlindRemotePins.ChannelUp, PinMode.Output);
            _controller.OpenPin((int)BlindRemotePins.ChannelDown, PinMode.Output);
        }

        /// <summary>
        /// Resets the power to the remote control.
        /// It ensures that the state of this instance is in sync with the physical remote's channel selection.
        /// </summary>
        public virtual async Task Reset()
        {
            ResetButtonPins();
            await ResetPower();
            // Workaround for first channel button press not working after reset.
            await WakeUp();
        }

        /// <summary>
        /// Sets the channel on the physical remote to the specified value.
        /// </summary>
        /// <param name="channel"></param>
        /// <exception cref="ArgumentException">Throws if channel number argument is invalid.</exception>
        public virtual async Task SetChannel(int channel)
        {
            if (_channel == channel)
            {
                return;
            }

            if (channel < MinChannel || channel > MaxChannel)
            {
                throw new ArgumentException(
                    $"Value cannot be less than {MinChannel} or greater than {MaxChannel}.", nameof(channel));
            }

            // This loop could be made more efficient in some cases by taking advantage of the wrap-around behaviour.
            // I.e. going from channel 0 to channel 15 could be achieved with only one channel-down button press.
            do
            {
                if (_channel < channel)
                {
                    await ChannelUp();
                }
                else
                {
                    await ChannelDown();
                }

            } while (_channel != channel);
        }

        /// <summary>
        /// Opens the blind(s) paired with the current channel.
        /// </summary>
        public virtual async Task Open()
        {
            _controller.Write((int)BlindRemotePins.Open, PinValue.High);
            await Task.Delay(ButtonPressMilliseconds);
            _controller.Write((int)BlindRemotePins.Open, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();
        }

        /// <summary>
        /// Closes the blind(s) paired with the current channel.
        /// </summary>
        public virtual async Task Close()
        {
            _controller.Write((int)BlindRemotePins.Close, PinValue.High);
            await Task.Delay(ButtonPressMilliseconds);
            _controller.Write((int)BlindRemotePins.Close, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();
        }

        /// <summary>
        /// Stops the blind(s) paired with the current channel.
        /// </summary>
        public virtual async Task Stop()
        {
            _controller.Write((int)BlindRemotePins.Stop, PinValue.High);
            await Task.Delay(ButtonPressMilliseconds);
            _controller.Write((int)BlindRemotePins.Stop, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();
        }

        /// <summary>
        /// Pairs or un-pairs a blind with the current channel.
        /// The pairing/un-pairing mode must be activated on the blind motor.
        /// </summary>
        public virtual async Task Pair()
        {
            _controller.Write((int)BlindRemotePins.Stop, PinValue.High);
            await Task.Delay(PairingMilliseconds);
            _controller.Write((int)BlindRemotePins.Stop, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();
        }

        /// <summary>
        /// Increments the current channel by one.
        /// </summary>
        /// <param name="wakeUp"></param>
        public virtual async Task ChannelUp()
        {
            await WakeUp();

            _controller.Write((int)BlindRemotePins.ChannelUp, PinValue.High);
            await Task.Delay(ButtonPressMilliseconds);
            _controller.Write((int)BlindRemotePins.ChannelUp, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();

            if (_channel < MaxChannel)
            {
                _channel++;
            }
            else
            {
                _channel = MinChannel;
            }
        }

        /// <summary>
        /// Decrements the current channel by one.
        /// </summary>
        /// <param name="wakeUp"></param>
        public virtual async Task ChannelDown()
        {
            await WakeUp();

            _controller.Write((int)BlindRemotePins.ChannelDown, PinValue.High);
            await Task.Delay(ButtonPressMilliseconds);
            _controller.Write((int)BlindRemotePins.ChannelDown, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();

            if (_channel > MinChannel)
            {
                _channel--;
            }
            else
            {
                _channel = MaxChannel;
            }
        }

        /// <summary>
        /// Used in normal operating mode to wake-up the MCU, without causing side-effects if already awake.
        /// This class does not support channel limit setting in programming mode. Max channel is always 15.
        /// </summary>
        public virtual async Task ChannelLimit()
        {
            _controller.Write((int)BlindRemotePins.ChannelUp, PinValue.High);
            _controller.Write((int)BlindRemotePins.ChannelDown, PinValue.High);
            await Task.Delay(ButtonPressMilliseconds);
            _controller.Write((int)BlindRemotePins.ChannelUp, PinValue.Low);
            _controller.Write((int)BlindRemotePins.ChannelDown, PinValue.Low);
            await Task.Delay(SwitchDebounceMilliseconds);

            OnButtonPressed();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_controller != null)
                    {
                        ResetButtonPins();
                    }
                }

                _disposed = true;
            }
        }

        protected virtual void ResetButtonPins()
        {
            _controller.Write((int)BlindRemotePins.Open, PinValue.Low);
            _controller.Write((int)BlindRemotePins.Close, PinValue.Low);
            _controller.Write((int)BlindRemotePins.Stop, PinValue.Low);
            _controller.Write((int)BlindRemotePins.ChannelUp, PinValue.Low);
            _controller.Write((int)BlindRemotePins.ChannelDown, PinValue.Low);
        }

        protected virtual async Task ResetPower()
        {
            // Only turn power off if power pin is high, to save time.
            if (_controller.Read((int)BlindRemotePins.Power) == PinValue.High)
            {
                // Bring remote out of sleep first, otherwise reset may not work due to low current draw.
                await WakeUp();

                // Set power pin low and wait long enough for the MCU to lose state.
                _controller.Write((int)BlindRemotePins.Power, PinValue.Low);
                await Task.Delay(BrownOutWaitMilliseconds);
            }

            // Set power pin high to and wait to allow the MCU to boot.
            _controller.Write((int)BlindRemotePins.Power, PinValue.High);
            await Task.Delay(StartUpWaitMilliseconds);

            _channel = 1;
            _lastButtonPressTime = null;
        }

        protected virtual void OnButtonPressed()
        {
            _lastButtonPressTime = DateTime.Now;
        }

        protected virtual async Task WakeUp()
        {
            var lastButtonPressTime = _lastButtonPressTime;

            if (lastButtonPressTime == null
                || lastButtonPressTime.Value.AddMilliseconds(SleepTimeOutMilliseconds) < DateTime.Now)
            {
                await ChannelLimit();
            }
        }
    }

    /// <summary>
    /// Mappings for blind remote buttons to logical GPIO pin numbers.
    /// </summary>
    public enum BlindRemotePins
    {
        Power = 0,
        Open = 5,
        Close = 6,
        Stop = 13,
        ChannelUp = 19,
        ChannelDown = 26
    }
}
