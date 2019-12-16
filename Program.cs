using System;
using System.IO.Ports;
using NAudio.CoreAudioApi;
using CommandLine;


namespace AudioToSerial
{
    class Program
    {
        static SerialPort _serialPort;
        static int LastPeakValue;
        static int outputBaseLine;
        static int outputMultiplier;
        static int refreshRate;
        static void Main(string[] args)
        {
            outputBaseLine = 0;
            outputMultiplier = 255;
            refreshRate = 30;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => HandleArguments(opts))
                .WithNotParsed<Options>((errs) => Console.WriteLine(errs));
            
            var device = askDevice();

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    _serialPort = new SerialPort
                    {
                        PortName = "COM"+i,
                        BaudRate = 9600,
                        ReadTimeout = 2000
                    };
                    _serialPort.Open();
                    _serialPort.Write("c");
                    if (_serialPort.ReadLine().Trim().Equals("c"))
                    {
                        Console.WriteLine(_serialPort);
                        Console.WriteLine("Connected to COM" + i);
                        break;
                    }
                    _serialPort.Close();
                } catch (Exception e)
                {
                    if (e.GetType() == new UnauthorizedAccessException().GetType())
                    {
                        Console.WriteLine("Couldn't connect to device, make sure it's not already being used by another application");
                        Environment.Exit(1);
                    }else if(e.GetType() == new System.IO.FileNotFoundException().GetType())
                    {
                        Console.WriteLine("No device found on COM" + i + " Trying next COM port");
                    }
                }
            }

            if (!_serialPort.IsOpen)
            {
                Console.WriteLine("Could not connect to device, is it connected??");
                Environment.Exit(2);
            }

            var keepAlive = true;
            while (keepAlive)
            {
                LastPeakValue = (int) Math.Round(((device.AudioMeterInformation.MasterPeakValue * outputMultiplier) + outputBaseLine));
                if (LastPeakValue > 255) LastPeakValue = 255; else if (LastPeakValue == outputBaseLine) LastPeakValue = 0;
                Console.WriteLine(LastPeakValue);
                sendSerialData(LastPeakValue);
                System.Threading.Thread.Sleep(1000 / refreshRate);
            }
        }

        static bool sendSerialData(int value)
        {
            try
            {
                _serialPort.Write(value.ToString());
                return true;
            } catch (Exception)
            {
                Console.WriteLine("Couldn't send new data, did you unplug the device?");
                Console.WriteLine("Application closing, please reconnect the device and open the application again.");
                Environment.Exit(3);
                return false;
            }
        }

        static MMDevice askDevice()
        {
            var enumerator = new MMDeviceEnumerator();

            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        }

        static void HandleArguments(Options opts)
        {
            outputBaseLine = opts.OutputBaseLine.HasValue ? (int)opts.OutputBaseLine : outputBaseLine;
            outputMultiplier = opts.OutputMultiplier.HasValue ? (int)opts.OutputMultiplier : outputMultiplier;
            refreshRate = opts.RefreshRate.HasValue ? (int)opts.RefreshRate : refreshRate;
        }
    }
}
