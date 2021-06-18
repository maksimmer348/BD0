namespace BD0.CP
{
    public class ComConfig
    {

        public int ChannelNum;
        public int BaudRate;
        public int ParityBit;
        public int StopBits;
        public bool DTR;


        public static ComConfig[] Default =
        {
                new ComConfig {ChannelNum = 0, BaudRate = 1, ParityBit = 0, StopBits = 1, DTR = false}
        };

        public static ComConfig DefaultConfig => Default[0];
    }
}

