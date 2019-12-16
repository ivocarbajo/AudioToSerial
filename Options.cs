using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace AudioToSerial
{
    class Options
    {
        [Option('b', "output-base-line", Required = false, HelpText = "Sets the minimum value if there is audio playing (0 will still be 0)")]
        public int? OutputBaseLine { get; set; }

        [Option('m', "output-multiplier", Required = false, HelpText = "Sets the multiplier for the output, this is effectively the maximum value")]
        public int? OutputMultiplier { get; set; }

        [Option('r', "refresh-rate", Required = false, HelpText = "Times to update device per second")]
        public int? RefreshRate { get; set; }
    }
}
