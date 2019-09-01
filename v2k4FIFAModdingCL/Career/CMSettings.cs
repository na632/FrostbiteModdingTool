using System;
using System.Collections.Generic;
using System.Text;
using v2k4FIFAModdingCL.CustomAttributes;

namespace v2k4FIFAModdingCL.Career
{
    [FIFAVersion("ALL")]
    public class CMSettings
    {
        [FIFAVersion("ALL")]
        public class Fitness
        {
            #region All FIFAs
            [FIFAVersion("ALL")]
            public double CONSTANT { get; set; }

            // 0 - 33
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_1 = 70;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_2 = 10;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_3 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_4 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_5 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_6 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_7 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_8 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_9 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_1_10 = 5;

            // 33 - 66
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_1 = 55;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_2 = 20;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_3 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_4 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_5 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_6 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_7 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_8 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_9 = 5;
            [FIFAVersion("ALL")]
            public int BASE_DAY_2_10 = 5;

            // 66 - 100
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_1 = 30;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_2 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_3 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_4 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_5 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_6 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_7 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_8 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_9 = 15;
            [FIFAVersion("ALL")]
            public int BASE_DAY_3_10 = 15;
            #endregion
        }

    }
}
