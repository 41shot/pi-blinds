using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace FourOneShot.Pi.Devices
{
    public class StubGpioDriver : GpioDriver
    {
        private readonly IDictionary<int, PinMode> _pinModes = new Dictionary<int, PinMode>();
        private readonly IDictionary<int, PinValue> _pinValues = new Dictionary<int, PinValue>();

        protected override int PinCount => 40;

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            Debug.WriteLine($"{nameof(AddCallbackForPinValueChangedEvent)}({pinNumber}, {eventTypes}, PinChangeEventHandler)");
        }

        protected override void ClosePin(int pinNumber)
        {
            Debug.WriteLine($"{nameof(ClosePin)}({pinNumber})");
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            Debug.WriteLine($"{nameof(ConvertPinNumberToLogicalNumberingScheme)}({pinNumber})");

            return pinNumber;
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            Debug.WriteLine($"{nameof(GetPinMode)}({pinNumber})");

            return _pinModes.ContainsKey(pinNumber)
                ? _pinModes[pinNumber]
                : 0;
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            Debug.WriteLine($"{nameof(IsPinModeSupported)}({pinNumber}, {mode})");

            return true;
        }

        protected override void OpenPin(int pinNumber)
        {
            Debug.WriteLine($"{nameof(OpenPin)}({pinNumber})");
        }

        protected override PinValue Read(int pinNumber)
        {
            return _pinValues.ContainsKey(pinNumber)
                ? _pinValues[pinNumber]
                : 0;
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            Debug.WriteLine($"{nameof(RemoveCallbackForPinValueChangedEvent)}({pinNumber}, PinChangeEventHandler)");
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            Debug.WriteLine($"{nameof(SetPinMode)}({pinNumber}, {mode})");

            _pinModes[pinNumber] = mode;
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"{nameof(WaitForEvent)}({pinNumber}, {eventTypes}, CancellationToken)");

            Thread.Sleep(100);

            return new WaitForEventResult
            {
                EventTypes = eventTypes,
                TimedOut = false
            };
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            Debug.WriteLine($"{nameof(SetPinMode)}({pinNumber}, {value})");

            _pinValues[pinNumber] = value;
        }
    }
}
