using Exchange.MarketUtils;
using Exchange.Poloniex;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using Strategy.SCTP_BREAKOUT;
using System;
using System.Collections.Generic;
using System.IO;

namespace Strategy.SCTP_BREAKOUTTests
{
    [TestFixture]
    public class Tests
    {
        //https://poloniex.com/public?command=returnChartData&currencyPair=BTC_XMR&end=1498780800&period=14400&start=1496275200

        //monero data
        private const string poloniexHistData = @"[{""date"":1496275200,""high"":0.0187887,""low"":0.0181983,""open"":0.01840099,""close"":0.01849,""volume"":242.95588092,""quoteVolume"":13167.71115445,""weightedAverage"":0.01845088},{""date"":1496289600,""high"":0.01861426,""low"":0.01794614,""open"":0.01848999,""close"":0.01852354,""volume"":410.87465618,""quoteVolume"":22430.21725456,""weightedAverage"":0.0183179},{""date"":1496304000,""high"":0.01856,""low"":0.01828,""open"":0.01842486,""close"":0.01839463,""volume"":216.0954005,""quoteVolume"":11724.85208083,""weightedAverage"":0.01843054},{""date"":1496318400,""high"":0.019,""low"":0.01837,""open"":0.01839,""close"":0.01884001,""volume"":492.52255028,""quoteVolume"":26302.17290032,""weightedAverage"":0.01872554},{""date"":1496332800,""high"":0.01894169,""low"":0.01855901,""open"":0.01885912,""close"":0.01865519,""volume"":227.00467038,""quoteVolume"":12127.01371864,""weightedAverage"":0.01871892},{""date"":1496347200,""high"":0.01877988,""low"":0.01821001,""open"":0.01865519,""close"":0.0183,""volume"":333.39765527,""quoteVolume"":18026.58409224,""weightedAverage"":0.01849477},{""date"":1496361600,""high"":0.01859928,""low"":0.0181015,""open"":0.01831499,""close"":0.01831023,""volume"":228.60537812,""quoteVolume"":12443.39871088,""weightedAverage"":0.01837161},{""date"":1496376000,""high"":0.01843284,""low"":0.0181,""open"":0.01831026,""close"":0.0182,""volume"":233.75581053,""quoteVolume"":12836.5667861,""weightedAverage"":0.01821015},{""date"":1496390400,""high"":0.01860705,""low"":0.0182,""open"":0.01820001,""close"":0.01859,""volume"":193.35402059,""quoteVolume"":10478.91356835,""weightedAverage"":0.01845172},{""date"":1496404800,""high"":0.0186521,""low"":0.01810401,""open"":0.01859,""close"":0.01810401,""volume"":286.05966919,""quoteVolume"":15602.45251609,""weightedAverage"":0.01833427},{""date"":1496419200,""high"":0.01823,""low"":0.0179001,""open"":0.01813556,""close"":0.01811557,""volume"":217.40289561,""quoteVolume"":12033.02817285,""weightedAverage"":0.01806718},{""date"":1496433600,""high"":0.01811556,""low"":0.01778722,""open"":0.01811556,""close"":0.01778723,""volume"":244.54192258,""quoteVolume"":13642.04584092,""weightedAverage"":0.0179256},{""date"":1496448000,""high"":0.018115,""low"":0.0177019,""open"":0.01778666,""close"":0.01799996,""volume"":189.49167657,""quoteVolume"":10591.38193275,""weightedAverage"":0.01789111},{""date"":1496462400,""high"":0.01805332,""low"":0.01770236,""open"":0.01788464,""close"":0.01781465,""volume"":138.64629638,""quoteVolume"":7770.80746966,""weightedAverage"":0.01784194},{""date"":1496476800,""high"":0.01794109,""low"":0.0174,""open"":0.01780321,""close"":0.01754531,""volume"":253.00044966,""quoteVolume"":14358.46397196,""weightedAverage"":0.0176203},{""date"":1496491200,""high"":0.01765,""low"":0.01706001,""open"":0.01745001,""close"":0.01721977,""volume"":305.42169948,""quoteVolume"":17603.16166235,""weightedAverage"":0.01735038},{""date"":1496505600,""high"":0.01736999,""low"":0.01696969,""open"":0.01729996,""close"":0.01718995,""volume"":339.29847929,""quoteVolume"":19796.3076115,""weightedAverage"":0.01713948},{""date"":1496520000,""high"":0.01718995,""low"":0.0165243,""open"":0.01718995,""close"":0.01672895,""volume"":342.98525298,""quoteVolume"":20409.68056553,""weightedAverage"":0.01680502},{""date"":1496534400,""high"":0.01733863,""low"":0.01661339,""open"":0.01672179,""close"":0.01733863,""volume"":206.65166356,""quoteVolume"":12151.49058398,""weightedAverage"":0.01700628},{""date"":1496548800,""high"":0.01741637,""low"":0.01677543,""open"":0.01733274,""close"":0.01693006,""volume"":145.14075156,""quoteVolume"":8504.9638468,""weightedAverage"":0.01706541},{""date"":1496563200,""high"":0.01704715,""low"":0.01666,""open"":0.01693007,""close"":0.01678958,""volume"":124.3245136,""quoteVolume"":7382.34080535,""weightedAverage"":0.01684079},{""date"":1496577600,""high"":0.0173124,""low"":0.01661312,""open"":0.01666066,""close"":0.01681237,""volume"":338.13176695,""quoteVolume"":19962.21524176,""weightedAverage"":0.01693858},{""date"":1496592000,""high"":0.01733,""low"":0.01666012,""open"":0.01687002,""close"":0.01702524,""volume"":388.05655272,""quoteVolume"":22879.13060073,""weightedAverage"":0.01696115},{""date"":1496606400,""high"":0.01731974,""low"":0.0169,""open"":0.01702525,""close"":0.0171,""volume"":228.84204272,""quoteVolume"":13401.96144584,""weightedAverage"":0.01707526},{""date"":1496620800,""high"":0.0173821,""low"":0.01706276,""open"":0.0171,""close"":0.017328,""volume"":171.11422942,""quoteVolume"":9944.9323315,""weightedAverage"":0.01720617},{""date"":1496635200,""high"":0.01798008,""low"":0.01725378,""open"":0.017328,""close"":0.0178822,""volume"":531.83164869,""quoteVolume"":30260.68787274,""weightedAverage"":0.017575},{""date"":1496649600,""high"":0.0182,""low"":0.0177748,""open"":0.01788231,""close"":0.01782002,""volume"":412.02871806,""quoteVolume"":22850.15955466,""weightedAverage"":0.01803176},{""date"":1496664000,""high"":0.01782002,""low"":0.01730774,""open"":0.01782002,""close"":0.01740691,""volume"":375.70228297,""quoteVolume"":21409.86750891,""weightedAverage"":0.01754809},{""date"":1496678400,""high"":0.01773557,""low"":0.017295,""open"":0.01746274,""close"":0.01760711,""volume"":265.67839016,""quoteVolume"":15199.58337119,""weightedAverage"":0.01747932},{""date"":1496692800,""high"":0.01834323,""low"":0.01745,""open"":0.01761,""close"":0.01804932,""volume"":566.66152726,""quoteVolume"":31438.71820364,""weightedAverage"":0.01802432},{""date"":1496707200,""high"":0.019,""low"":0.01689998,""open"":0.01804932,""close"":0.01827751,""volume"":2207.00277386,""quoteVolume"":122208.40950919,""weightedAverage"":0.01805933},{""date"":1496721600,""high"":0.01894964,""low"":0.01757851,""open"":0.01827751,""close"":0.01868,""volume"":978.93179908,""quoteVolume"":53723.68838336,""weightedAverage"":0.0182216},{""date"":1496736000,""high"":0.019,""low"":0.01794001,""open"":0.01868001,""close"":0.01825881,""volume"":869.235303,""quoteVolume"":46852.86338225,""weightedAverage"":0.01855244},{""date"":1496750400,""high"":0.01851569,""low"":0.01799718,""open"":0.01825881,""close"":0.01799721,""volume"":436.95856896,""quoteVolume"":23920.18049205,""weightedAverage"":0.01826736},{""date"":1496764800,""high"":0.01818301,""low"":0.01751977,""open"":0.01799721,""close"":0.0178001,""volume"":464.87935919,""quoteVolume"":26096.77745243,""weightedAverage"":0.01781366},{""date"":1496779200,""high"":0.0197,""low"":0.01752501,""open"":0.01780009,""close"":0.01942,""volume"":1320.78854245,""quoteVolume"":70352.76404595,""weightedAverage"":0.01877379},{""date"":1496793600,""high"":0.02231,""low"":0.0192801,""open"":0.01942,""close"":0.02020209,""volume"":5833.12958761,""quoteVolume"":280625.22053899,""weightedAverage"":0.02078619},{""date"":1496808000,""high"":0.02099498,""low"":0.01895379,""open"":0.02020222,""close"":0.01978715,""volume"":1976.09278348,""quoteVolume"":99828.02934306,""weightedAverage"":0.01979496},{""date"":1496822400,""high"":0.02049189,""low"":0.01923801,""open"":0.019601,""close"":0.02019277,""volume"":1108.65917901,""quoteVolume"":55844.977319,""weightedAverage"":0.01985244},{""date"":1496836800,""high"":0.02049188,""low"":0.01928666,""open"":0.02019278,""close"":0.01957997,""volume"":1028.69369518,""quoteVolume"":51951.92000921,""weightedAverage"":0.01980087},{""date"":1496851200,""high"":0.02048393,""low"":0.019479,""open"":0.01950003,""close"":0.02039,""volume"":522.83630423,""quoteVolume"":26066.83128143,""weightedAverage"":0.02005753},{""date"":1496865600,""high"":0.02050698,""low"":0.01950122,""open"":0.02039,""close"":0.01961141,""volume"":823.06944988,""quoteVolume"":40996.8907618,""weightedAverage"":0.02007638},{""date"":1496880000,""high"":0.01999898,""low"":0.0195,""open"":0.01969878,""close"":0.01988773,""volume"":417.5836534,""quoteVolume"":21179.76545969,""weightedAverage"":0.01971616},{""date"":1496894400,""high"":0.020012,""low"":0.01968,""open"":0.01988774,""close"":0.0198999,""volume"":203.78961027,""quoteVolume"":10261.26695271,""weightedAverage"":0.01986008},{""date"":1496908800,""high"":0.0200481,""low"":0.01966376,""open"":0.01989991,""close"":0.01979109,""volume"":231.61743925,""quoteVolume"":11669.45563923,""weightedAverage"":0.01984817},{""date"":1496923200,""high"":0.02043,""low"":0.01952114,""open"":0.0197911,""close"":0.01999998,""volume"":648.04926198,""quoteVolume"":32538.05344834,""weightedAverage"":0.01991665},{""date"":1496937600,""high"":0.02069917,""low"":0.0199174,""open"":0.01995267,""close"":0.02026992,""volume"":434.53962906,""quoteVolume"":21317.87195541,""weightedAverage"":0.02038381},{""date"":1496952000,""high"":0.0205,""low"":0.0199075,""open"":0.02027923,""close"":0.01996532,""volume"":413.95104314,""quoteVolume"":20506.92372643,""weightedAverage"":0.02018591},{""date"":1496966400,""high"":0.02009223,""low"":0.0193,""open"":0.01996532,""close"":0.01981717,""volume"":391.49388818,""quoteVolume"":19914.39853146,""weightedAverage"":0.01965883},{""date"":1496980800,""high"":0.0199,""low"":0.01964988,""open"":0.01984092,""close"":0.01989997,""volume"":149.67808597,""quoteVolume"":7570.59526085,""weightedAverage"":0.01977097},{""date"":1496995200,""high"":0.02016254,""low"":0.0196499,""open"":0.01987501,""close"":0.02001,""volume"":361.73640811,""quoteVolume"":18195.08145928,""weightedAverage"":0.01988099},{""date"":1497009600,""high"":0.02016245,""low"":0.01980001,""open"":0.02001,""close"":0.0199678,""volume"":284.41586806,""quoteVolume"":14213.74059517,""weightedAverage"":0.02000992},{""date"":1497024000,""high"":0.02022222,""low"":0.01968001,""open"":0.0199678,""close"":0.01989996,""volume"":278.14885934,""quoteVolume"":13932.10678141,""weightedAverage"":0.01996459},{""date"":1497038400,""high"":0.0200088,""low"":0.01967999,""open"":0.01985964,""close"":0.01974249,""volume"":185.25821499,""quoteVolume"":9357.27507508,""weightedAverage"":0.0197983},{""date"":1497052800,""high"":0.01989866,""low"":0.01964988,""open"":0.01982997,""close"":0.01980004,""volume"":147.63575978,""quoteVolume"":7484.22855557,""weightedAverage"":0.01972624},{""date"":1497067200,""high"":0.0199,""low"":0.01953737,""open"":0.01980014,""close"":0.019625,""volume"":262.87549932,""quoteVolume"":13372.02995398,""weightedAverage"":0.0196586},{""date"":1497081600,""high"":0.01973,""low"":0.01889,""open"":0.01962501,""close"":0.01889001,""volume"":466.58315995,""quoteVolume"":24217.44275156,""weightedAverage"":0.0192664},{""date"":1497096000,""high"":0.0191,""low"":0.0185,""open"":0.01889001,""close"":0.01878,""volume"":327.03671633,""quoteVolume"":17392.89794774,""weightedAverage"":0.01880288},{""date"":1497110400,""high"":0.01890009,""low"":0.01852542,""open"":0.01878,""close"":0.01885,""volume"":260.93465292,""quoteVolume"":13943.67211707,""weightedAverage"":0.01871348},{""date"":1497124800,""high"":0.01885584,""low"":0.01815003,""open"":0.01885,""close"":0.01820007,""volume"":342.04798811,""quoteVolume"":18543.61464766,""weightedAverage"":0.01844559},{""date"":1497139200,""high"":0.01865318,""low"":0.01810004,""open"":0.01820007,""close"":0.01865153,""volume"":223.73124174,""quoteVolume"":12220.33243818,""weightedAverage"":0.01830811},{""date"":1497153600,""high"":0.018891,""low"":0.01850875,""open"":0.01865153,""close"":0.01874998,""volume"":188.48723693,""quoteVolume"":10082.96545581,""weightedAverage"":0.01869363},{""date"":1497168000,""high"":0.01889668,""low"":0.01848001,""open"":0.01874999,""close"":0.01886522,""volume"":172.541472,""quoteVolume"":9197.28577536,""weightedAverage"":0.01876004},{""date"":1497182400,""high"":0.019851,""low"":0.01874001,""open"":0.01886522,""close"":0.0190309,""volume"":697.43979949,""quoteVolume"":36087.38007865,""weightedAverage"":0.01932641},{""date"":1497196800,""high"":0.01921248,""low"":0.01871386,""open"":0.0190309,""close"":0.01891911,""volume"":283.36629653,""quoteVolume"":14912.09638305,""weightedAverage"":0.01900244},{""date"":1497211200,""high"":0.01991685,""low"":0.0189,""open"":0.01899507,""close"":0.0199011,""volume"":401.44312431,""quoteVolume"":20644.81830399,""weightedAverage"":0.01944522},{""date"":1497225600,""high"":0.02022638,""low"":0.01958218,""open"":0.0199011,""close"":0.01971,""volume"":515.54114923,""quoteVolume"":25828.351098,""weightedAverage"":0.01996028},{""date"":1497240000,""high"":0.01983096,""low"":0.01931232,""open"":0.01971,""close"":0.01962713,""volume"":282.71931958,""quoteVolume"":14453.14969159,""weightedAverage"":0.01956108},{""date"":1497254400,""high"":0.019955,""low"":0.0187666,""open"":0.01962725,""close"":0.018962,""volume"":787.98661504,""quoteVolume"":40803.59876238,""weightedAverage"":0.01931169},{""date"":1497268800,""high"":0.01950503,""low"":0.0188253,""open"":0.01883529,""close"":0.01949869,""volume"":393.21474323,""quoteVolume"":20566.11722935,""weightedAverage"":0.01911954},{""date"":1497283200,""high"":0.0195,""low"":0.01832007,""open"":0.01949869,""close"":0.01884275,""volume"":816.44446523,""quoteVolume"":43434.46439372,""weightedAverage"":0.01879715},{""date"":1497297600,""high"":0.0194,""low"":0.0188,""open"":0.01888002,""close"":0.01900155,""volume"":358.46262777,""quoteVolume"":18714.27139677,""weightedAverage"":0.0191545},{""date"":1497312000,""high"":0.01947649,""low"":0.01871471,""open"":0.01909999,""close"":0.01934,""volume"":354.67683388,""quoteVolume"":18588.12853228,""weightedAverage"":0.01908082},{""date"":1497326400,""high"":0.019461,""low"":0.0187452,""open"":0.01933999,""close"":0.01906025,""volume"":318.20408037,""quoteVolume"":16587.81600449,""weightedAverage"":0.019183},{""date"":1497340800,""high"":0.01933998,""low"":0.018752,""open"":0.01906027,""close"":0.019025,""volume"":321.2421287,""quoteVolume"":16887.35068767,""weightedAverage"":0.01902264},{""date"":1497355200,""high"":0.01910455,""low"":0.01881328,""open"":0.01903104,""close"":0.0191,""volume"":259.38688185,""quoteVolume"":13659.9541963,""weightedAverage"":0.01898885},{""date"":1497369600,""high"":0.019401,""low"":0.01907006,""open"":0.0191,""close"":0.01918863,""volume"":318.38578842,""quoteVolume"":16507.29515591,""weightedAverage"":0.01928758},{""date"":1497384000,""high"":0.01939,""low"":0.01901425,""open"":0.01928827,""close"":0.019225,""volume"":270.93735478,""quoteVolume"":14103.04873382,""weightedAverage"":0.01921126},{""date"":1497398400,""high"":0.01944409,""low"":0.01910581,""open"":0.01922501,""close"":0.01938,""volume"":211.64410452,""quoteVolume"":10990.65041643,""weightedAverage"":0.01925674},{""date"":1497412800,""high"":0.01944444,""low"":0.01919963,""open"":0.01937041,""close"":0.01919963,""volume"":173.85694967,""quoteVolume"":9011.9094647,""weightedAverage"":0.01929191},{""date"":1497427200,""high"":0.01938684,""low"":0.01909606,""open"":0.01919964,""close"":0.01919999,""volume"":201.21403728,""quoteVolume"":10472.84267826,""weightedAverage"":0.01921293},{""date"":1497441600,""high"":0.0192,""low"":0.0188088,""open"":0.01919999,""close"":0.01891001,""volume"":363.16855339,""quoteVolume"":19099.12706598,""weightedAverage"":0.01901492},{""date"":1497456000,""high"":0.01912221,""low"":0.018867,""open"":0.01891001,""close"":0.018943,""volume"":203.86808385,""quoteVolume"":10736.76848887,""weightedAverage"":0.01898784},{""date"":1497470400,""high"":0.01909836,""low"":0.01880026,""open"":0.018943,""close"":0.01890393,""volume"":270.83099745,""quoteVolume"":14290.88100934,""weightedAverage"":0.01895131},{""date"":1497484800,""high"":0.01896048,""low"":0.0186,""open"":0.0188705,""close"":0.01879869,""volume"":228.12278203,""quoteVolume"":12144.2885863,""weightedAverage"":0.01878436},{""date"":1497499200,""high"":0.0190441,""low"":0.0185,""open"":0.01874511,""close"":0.01869176,""volume"":362.44396533,""quoteVolume"":19411.9849408,""weightedAverage"":0.01867114},{""date"":1497513600,""high"":0.01898838,""low"":0.018591,""open"":0.01869176,""close"":0.01869302,""volume"":161.9613479,""quoteVolume"":8634.68070856,""weightedAverage"":0.01875707},{""date"":1497528000,""high"":0.01907122,""low"":0.01820001,""open"":0.01870003,""close"":0.01888,""volume"":550.38552604,""quoteVolume"":29584.63680073,""weightedAverage"":0.01860376},{""date"":1497542400,""high"":0.0191,""low"":0.01855334,""open"":0.01901573,""close"":0.01871841,""volume"":291.06554702,""quoteVolume"":15398.2724742,""weightedAverage"":0.01890248},{""date"":1497556800,""high"":0.01923647,""low"":0.018668,""open"":0.01871842,""close"":0.01902,""volume"":344.89190979,""quoteVolume"":18171.15900105,""weightedAverage"":0.01898018},{""date"":1497571200,""high"":0.01929898,""low"":0.01889318,""open"":0.01902003,""close"":0.01892013,""volume"":167.63910262,""quoteVolume"":8776.08394495,""weightedAverage"":0.01910181},{""date"":1497585600,""high"":0.01913183,""low"":0.01888436,""open"":0.01892013,""close"":0.01910701,""volume"":126.78156419,""quoteVolume"":6664.55421172,""weightedAverage"":0.01902326},{""date"":1497600000,""high"":0.01925,""low"":0.01886104,""open"":0.01910701,""close"":0.01909194,""volume"":226.48663403,""quoteVolume"":11875.87505893,""weightedAverage"":0.01907115},{""date"":1497614400,""high"":0.01932,""low"":0.01892068,""open"":0.01909195,""close"":0.01921001,""volume"":201.76091063,""quoteVolume"":10524.05518982,""weightedAverage"":0.0191714},{""date"":1497628800,""high"":0.01931905,""low"":0.0191,""open"":0.01921001,""close"":0.019205,""volume"":212.96264385,""quoteVolume"":11089.90008667,""weightedAverage"":0.01920329},{""date"":1497643200,""high"":0.01949506,""low"":0.01916876,""open"":0.019205,""close"":0.01934,""volume"":236.44159924,""quoteVolume"":12218.54739126,""weightedAverage"":0.01935103},{""date"":1497657600,""high"":0.02,""low"":0.01931268,""open"":0.01934001,""close"":0.0197,""volume"":557.41578442,""quoteVolume"":28242.32184195,""weightedAverage"":0.01973689},{""date"":1497672000,""high"":0.02059917,""low"":0.0195011,""open"":0.019694,""close"":0.0205901,""volume"":577.0727804,""quoteVolume"":28810.92357895,""weightedAverage"":0.02002965},{""date"":1497686400,""high"":0.02059998,""low"":0.01972226,""open"":0.02059009,""close"":0.019902,""volume"":459.73791305,""quoteVolume"":22870.63228761,""weightedAverage"":0.02010167},{""date"":1497700800,""high"":0.01998988,""low"":0.0195,""open"":0.019902,""close"":0.01967747,""volume"":509.68854925,""quoteVolume"":25796.68116209,""weightedAverage"":0.01975791},{""date"":1497715200,""high"":0.01988682,""low"":0.0195,""open"":0.01967747,""close"":0.01988299,""volume"":364.19719679,""quoteVolume"":18533.29820078,""weightedAverage"":0.01965096},{""date"":1497729600,""high"":0.0201,""low"":0.01955923,""open"":0.01974,""close"":0.01999303,""volume"":375.49820715,""quoteVolume"":18915.12685812,""weightedAverage"":0.01985174},{""date"":1497744000,""high"":0.0204084,""low"":0.0198,""open"":0.01999303,""close"":0.02005056,""volume"":392.73362785,""quoteVolume"":19502.38750236,""weightedAverage"":0.02013772},{""date"":1497758400,""high"":0.02049999,""low"":0.02003195,""open"":0.02005057,""close"":0.02032129,""volume"":322.17449959,""quoteVolume"":15843.03537378,""weightedAverage"":0.0203354},{""date"":1497772800,""high"":0.02035937,""low"":0.0199,""open"":0.02032131,""close"":0.01998639,""volume"":278.27893781,""quoteVolume"":13885.07716375,""weightedAverage"":0.02004158},{""date"":1497787200,""high"":0.02039999,""low"":0.01998639,""open"":0.0199864,""close"":0.02014979,""volume"":199.51796544,""quoteVolume"":9864.01908189,""weightedAverage"":0.02022684},{""date"":1497801600,""high"":0.0203,""low"":0.0198,""open"":0.02009918,""close"":0.01988499,""volume"":218.09422902,""quoteVolume"":10875.26196284,""weightedAverage"":0.02005415},{""date"":1497816000,""high"":0.01992001,""low"":0.01962019,""open"":0.01992001,""close"":0.01986847,""volume"":179.73954763,""quoteVolume"":9078.89518435,""weightedAverage"":0.01979751},{""date"":1497830400,""high"":0.02022152,""low"":0.01981,""open"":0.01986847,""close"":0.02020974,""volume"":219.69320565,""quoteVolume"":10975.991196,""weightedAverage"":0.02001579},{""date"":1497844800,""high"":0.0202226,""low"":0.0196,""open"":0.02020995,""close"":0.01961909,""volume"":326.58227269,""quoteVolume"":16414.52284486,""weightedAverage"":0.01989593},{""date"":1497859200,""high"":0.02009006,""low"":0.01960223,""open"":0.01963,""close"":0.02008001,""volume"":281.41652969,""quoteVolume"":14191.54361311,""weightedAverage"":0.01982987},{""date"":1497873600,""high"":0.02009007,""low"":0.01916869,""open"":0.02008001,""close"":0.0195,""volume"":825.41761637,""quoteVolume"":42258.2663453,""weightedAverage"":0.01953268},{""date"":1497888000,""high"":0.01966937,""low"":0.0192,""open"":0.019477,""close"":0.01958597,""volume"":433.20777486,""quoteVolume"":22273.36661305,""weightedAverage"":0.01944958},{""date"":1497902400,""high"":0.01978497,""low"":0.01945614,""open"":0.01953,""close"":0.019489,""volume"":324.72575765,""quoteVolume"":16518.32566273,""weightedAverage"":0.01965851},{""date"":1497916800,""high"":0.01982531,""low"":0.01923499,""open"":0.019489,""close"":0.01960726,""volume"":391.59822446,""quoteVolume"":20021.85245887,""weightedAverage"":0.01955854},{""date"":1497931200,""high"":0.01969,""low"":0.01946002,""open"":0.01960726,""close"":0.0195968,""volume"":229.85862928,""quoteVolume"":11752.2785612,""weightedAverage"":0.01955864},{""date"":1497945600,""high"":0.01999091,""low"":0.01949009,""open"":0.01950112,""close"":0.01989007,""volume"":476.28859561,""quoteVolume"":24044.49609234,""weightedAverage"":0.01980863},{""date"":1497960000,""high"":0.02022785,""low"":0.01984993,""open"":0.01989008,""close"":0.01995,""volume"":630.68908417,""quoteVolume"":31480.03493166,""weightedAverage"":0.02003457},{""date"":1497974400,""high"":0.020066,""low"":0.01900066,""open"":0.0199809,""close"":0.01900066,""volume"":712.90719232,""quoteVolume"":36711.15786948,""weightedAverage"":0.01941936},{""date"":1497988800,""high"":0.01927613,""low"":0.0187,""open"":0.0190022,""close"":0.01900002,""volume"":593.56476622,""quoteVolume"":31434.08792438,""weightedAverage"":0.01888283},{""date"":1498003200,""high"":0.01931343,""low"":0.01885801,""open"":0.01900004,""close"":0.01890016,""volume"":220.76280531,""quoteVolume"":11577.01102016,""weightedAverage"":0.01906906},{""date"":1498017600,""high"":0.01905752,""low"":0.01870115,""open"":0.01890016,""close"":0.01872012,""volume"":284.45410278,""quoteVolume"":15081.39393348,""weightedAverage"":0.01886126},{""date"":1498032000,""high"":0.01894998,""low"":0.01854125,""open"":0.01874,""close"":0.01861028,""volume"":383.0328734,""quoteVolume"":20431.30441963,""weightedAverage"":0.01874735},{""date"":1498046400,""high"":0.01905534,""low"":0.0184,""open"":0.01863,""close"":0.01889893,""volume"":403.61304943,""quoteVolume"":21565.57934561,""weightedAverage"":0.01871561},{""date"":1498060800,""high"":0.01923726,""low"":0.01860023,""open"":0.01905521,""close"":0.01875888,""volume"":228.943895,""quoteVolume"":12066.35890823,""weightedAverage"":0.01897373},{""date"":1498075200,""high"":0.01887997,""low"":0.01780012,""open"":0.01887132,""close"":0.01840004,""volume"":651.23267277,""quoteVolume"":35609.63049393,""weightedAverage"":0.0182881},{""date"":1498089600,""high"":0.01873799,""low"":0.01819463,""open"":0.01840004,""close"":0.01849984,""volume"":178.87074835,""quoteVolume"":9651.60793578,""weightedAverage"":0.01853274},{""date"":1498104000,""high"":0.01868779,""low"":0.01840004,""open"":0.0184574,""close"":0.01855,""volume"":157.96019609,""quoteVolume"":8512.09667067,""weightedAverage"":0.01855714},{""date"":1498118400,""high"":0.01878084,""low"":0.0184013,""open"":0.01855001,""close"":0.01875999,""volume"":210.3917734,""quoteVolume"":11285.97242689,""weightedAverage"":0.01864188},{""date"":1498132800,""high"":0.01884001,""low"":0.0184014,""open"":0.01876,""close"":0.01845073,""volume"":219.64184026,""quoteVolume"":11774.93506222,""weightedAverage"":0.01865333},{""date"":1498147200,""high"":0.01872463,""low"":0.01844003,""open"":0.01845073,""close"":0.0186008,""volume"":109.04726073,""quoteVolume"":5871.24281507,""weightedAverage"":0.01857311},{""date"":1498161600,""high"":0.01880998,""low"":0.0185,""open"":0.0186008,""close"":0.01863593,""volume"":117.02209733,""quoteVolume"":6267.32819383,""weightedAverage"":0.01867176},{""date"":1498176000,""high"":0.0187979,""low"":0.01844016,""open"":0.01863593,""close"":0.01850202,""volume"":204.37504215,""quoteVolume"":11002.54973871,""weightedAverage"":0.01857524},{""date"":1498190400,""high"":0.01885277,""low"":0.01845,""open"":0.01850203,""close"":0.01884764,""volume"":187.70129628,""quoteVolume"":10051.73776249,""weightedAverage"":0.01867351},{""date"":1498204800,""high"":0.01915,""low"":0.0187241,""open"":0.01884764,""close"":0.01908571,""volume"":285.90659424,""quoteVolume"":15062.28538733,""weightedAverage"":0.01898162},{""date"":1498219200,""high"":0.01913,""low"":0.01862005,""open"":0.01900001,""close"":0.01883266,""volume"":274.29006195,""quoteVolume"":14489.88449181,""weightedAverage"":0.01892976},{""date"":1498233600,""high"":0.01924908,""low"":0.01870011,""open"":0.01885162,""close"":0.0192,""volume"":168.82663871,""quoteVolume"":8907.35920977,""weightedAverage"":0.01895361},{""date"":1498248000,""high"":0.01924972,""low"":0.01902,""open"":0.0192,""close"":0.01902731,""volume"":97.49208358,""quoteVolume"":5102.82742135,""weightedAverage"":0.0191055},{""date"":1498262400,""high"":0.0191711,""low"":0.01876,""open"":0.01902731,""close"":0.01887312,""volume"":143.61356024,""quoteVolume"":7602.52439849,""weightedAverage"":0.01889024},{""date"":1498276800,""high"":0.01893281,""low"":0.018698,""open"":0.01889365,""close"":0.01879999,""volume"":180.47678108,""quoteVolume"":9604.55407421,""weightedAverage"":0.01879075},{""date"":1498291200,""high"":0.01880552,""low"":0.01853603,""open"":0.01870301,""close"":0.0186118,""volume"":177.13898876,""quoteVolume"":9484.42055755,""weightedAverage"":0.01867683},{""date"":1498305600,""high"":0.01879105,""low"":0.0185022,""open"":0.0186118,""close"":0.01867499,""volume"":138.16035788,""quoteVolume"":7412.19254663,""weightedAverage"":0.0186396},{""date"":1498320000,""high"":0.01883999,""low"":0.01854153,""open"":0.0187,""close"":0.018785,""volume"":117.47783871,""quoteVolume"":6283.19403194,""weightedAverage"":0.01869715},{""date"":1498334400,""high"":0.01893281,""low"":0.01840468,""open"":0.01878499,""close"":0.0186,""volume"":263.597278,""quoteVolume"":14128.91654338,""weightedAverage"":0.01865658},{""date"":1498348800,""high"":0.01866463,""low"":0.01840141,""open"":0.0185995,""close"":0.01848352,""volume"":137.23741546,""quoteVolume"":7408.54605972,""weightedAverage"":0.0185242},{""date"":1498363200,""high"":0.01849976,""low"":0.0184,""open"":0.01848353,""close"":0.01842001,""volume"":102.01774378,""quoteVolume"":5531.57908133,""weightedAverage"":0.01844278},{""date"":1498377600,""high"":0.01852628,""low"":0.01833348,""open"":0.01843,""close"":0.01851,""volume"":125.04270526,""quoteVolume"":6781.33783305,""weightedAverage"":0.01843923},{""date"":1498392000,""high"":0.0185104,""low"":0.01822501,""open"":0.01851001,""close"":0.01826607,""volume"":176.56241438,""quoteVolume"":9598.46366603,""weightedAverage"":0.01839486},{""date"":1498406400,""high"":0.01840527,""low"":0.0182,""open"":0.01833935,""close"":0.018394,""volume"":102.50301284,""quoteVolume"":5601.15515376,""weightedAverage"":0.01830033},{""date"":1498420800,""high"":0.01869752,""low"":0.01795446,""open"":0.018394,""close"":0.01802001,""volume"":410.83640227,""quoteVolume"":22509.66061812,""weightedAverage"":0.01825155},{""date"":1498435200,""high"":0.01883515,""low"":0.018,""open"":0.01802002,""close"":0.01871866,""volume"":383.0025865,""quoteVolume"":20619.0036891,""weightedAverage"":0.01857522},{""date"":1498449600,""high"":0.01879996,""low"":0.01810011,""open"":0.01874673,""close"":0.01824839,""volume"":334.31323005,""quoteVolume"":18088.31899073,""weightedAverage"":0.01848227},{""date"":1498464000,""high"":0.01828397,""low"":0.01760002,""open"":0.0182246,""close"":0.01761035,""volume"":313.64770219,""quoteVolume"":17539.32353964,""weightedAverage"":0.01788254},{""date"":1498478400,""high"":0.01787685,""low"":0.0171,""open"":0.01761035,""close"":0.01711151,""volume"":460.16518251,""quoteVolume"":26223.19707921,""weightedAverage"":0.01754801},{""date"":1498492800,""high"":0.01733551,""low"":0.0169,""open"":0.017111,""close"":0.01716998,""volume"":536.5785083,""quoteVolume"":31463.7937006,""weightedAverage"":0.01705384},{""date"":1498507200,""high"":0.01796524,""low"":0.01688,""open"":0.01716998,""close"":0.01794074,""volume"":453.47807067,""quoteVolume"":26144.61675982,""weightedAverage"":0.01734498},{""date"":1498521600,""high"":0.01810872,""low"":0.0168,""open"":0.01794075,""close"":0.01751148,""volume"":314.62821236,""quoteVolume"":17829.2095887,""weightedAverage"":0.01764678},{""date"":1498536000,""high"":0.01774999,""low"":0.01685,""open"":0.01751148,""close"":0.01690636,""volume"":249.13917792,""quoteVolume"":14481.8936506,""weightedAverage"":0.01720349},{""date"":1498550400,""high"":0.01724812,""low"":0.01682521,""open"":0.01696289,""close"":0.01713823,""volume"":310.65694111,""quoteVolume"":18271.1738536,""weightedAverage"":0.01700257},{""date"":1498564800,""high"":0.01721678,""low"":0.01665001,""open"":0.01713823,""close"":0.01673788,""volume"":289.82505322,""quoteVolume"":17149.14545672,""weightedAverage"":0.01690026},{""date"":1498579200,""high"":0.01744768,""low"":0.01670002,""open"":0.01673787,""close"":0.01726005,""volume"":266.97561268,""quoteVolume"":15617.75372644,""weightedAverage"":0.01709436},{""date"":1498593600,""high"":0.01752,""low"":0.01717734,""open"":0.01726006,""close"":0.01720007,""volume"":232.37890527,""quoteVolume"":13341.73025793,""weightedAverage"":0.01741744},{""date"":1498608000,""high"":0.01745,""low"":0.017019,""open"":0.01720008,""close"":0.01722998,""volume"":214.54666096,""quoteVolume"":12488.42721351,""weightedAverage"":0.01717963},{""date"":1498622400,""high"":0.01776649,""low"":0.01715501,""open"":0.01715502,""close"":0.0177,""volume"":230.82193241,""quoteVolume"":13179.61398009,""weightedAverage"":0.01751355},{""date"":1498636800,""high"":0.01798005,""low"":0.0175013,""open"":0.0177,""close"":0.01787082,""volume"":190.85813031,""quoteVolume"":10721.05952592,""weightedAverage"":0.01780217},{""date"":1498651200,""high"":0.018,""low"":0.01774813,""open"":0.01787083,""close"":0.01774813,""volume"":284.56772006,""quoteVolume"":15892.68405441,""weightedAverage"":0.01790557},{""date"":1498665600,""high"":0.01804,""low"":0.017502,""open"":0.01774813,""close"":0.01802,""volume"":209.58086722,""quoteVolume"":11793.46662282,""weightedAverage"":0.01777092},{""date"":1498680000,""high"":0.01851962,""low"":0.01787299,""open"":0.01802,""close"":0.01845487,""volume"":271.2774138,""quoteVolume"":14844.57491507,""weightedAverage"":0.01827451},{""date"":1498694400,""high"":0.01887881,""low"":0.01826002,""open"":0.01846581,""close"":0.01865001,""volume"":397.33848588,""quoteVolume"":21355.72006683,""weightedAverage"":0.01860571},{""date"":1498708800,""high"":0.01874359,""low"":0.01836274,""open"":0.01865001,""close"":0.01838467,""volume"":130.05168431,""quoteVolume"":6993.93276534,""weightedAverage"":0.01859492},{""date"":1498723200,""high"":0.01850591,""low"":0.01778371,""open"":0.01838467,""close"":0.01796469,""volume"":316.89869216,""quoteVolume"":17491.47931279,""weightedAverage"":0.01811731},{""date"":1498737600,""high"":0.01821512,""low"":0.01780619,""open"":0.01796521,""close"":0.01784016,""volume"":197.5609928,""quoteVolume"":10962.09863549,""weightedAverage"":0.01802218},{""date"":1498752000,""high"":0.01814819,""low"":0.01780569,""open"":0.01784017,""close"":0.01791527,""volume"":126.75947957,""quoteVolume"":7038.12661799,""weightedAverage"":0.0180104},{""date"":1498766400,""high"":0.01801018,""low"":0.01770122,""open"":0.0179153,""close"":0.01781817,""volume"":148.47638851,""quoteVolume"":8324.28783467,""weightedAverage"":0.01783652},{""date"":1498780800,""high"":0.01794086,""low"":0.01765102,""open"":0.01781817,""close"":0.01784585,""volume"":117.28819837,""quoteVolume"":6588.78145484,""weightedAverage"":0.01780119}]";

        private string poloniexFeesJson = @"{""transfer"":[{""currency"":""any"",""incoming"":0,""outgoing"":0,""feeType"":""absolute""}],""transaction"":[{""treshold"":600,""maker"":0.0015,""taker"":0.0025,""feeType"":""percentage""},{""treshold"":1200,""maker"":0.0014,""taker"":0.0024,""feeType"":""percentage""},{""treshold"":2400,""maker"":0.0012,""taker"":0.0022,""feeType"":""percentage""},{""treshold"":6000,""maker"":0.001,""taker"":0.002,""feeType"":""percentage""},{""treshold"":12000,""maker"":0.0008,""taker"":0.0016,""feeType"":""percentage""},{""treshold"":18000,""maker"":0.0005,""taker"":0.0014,""feeType"":""percentage""},{""treshold"":24000,""maker"":0.0002,""taker"":0.0012,""feeType"":""percentage""},{""treshold"":60000,""maker"":0,""taker"":0.001,""feeType"":""percentage""},{""treshold"":120000,""maker"":0,""taker"":0.0008,""feeType"":""percentage""},{""treshold"":999999999999999,""maker"":0,""taker"":0.0005,""feeType"":""percentage""}]}";
        private string logFile = @"C:\temp\test\log.txt";
        private decimal startBalance = 1000;

        [TestCase]
        public void TestSomething()
        {
            System.Net.WebRequest.DefaultWebProxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            var pair = new Tuple<string, string>(Currencies.Bitcoin, Currencies.Monero);

            var exchange = new Poloniex("https://poloniex.com/public", poloniexFeesJson, 0);
            int candlePeriodSeconds = 1800;
            TimeSpan analyzedTimeWindow = new TimeSpan(240, 0, 0);
            int candlesInTimeframe = (int) Math.Floor(analyzedTimeWindow.TotalSeconds / candlePeriodSeconds)/2; //take half of full window

            string candlesJson = exchange.GetHistoricalData(pair, DateTime.Now - analyzedTimeWindow, DateTime.Now, candlePeriodSeconds).GetAwaiter().GetResult().Item2;

            List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(candlesJson);

            ILog log = new ConsoleLogger(logFile);

            SctpBreakout breakoutStrategy = new SctpBreakout(
                exchange,
                new List<Tuple<string, string>>(),
                null,
                new TimeSpan(0, 0, candlePeriodSeconds),
                candlesInTimeframe,
                new decimal(0.05),
                startBalance,
                log);

            bool isInverted = false;
            var ordered = exchange.MakeValidPair(pair.Item1, pair.Item2, out isInverted);

            MarketToProcess market;

            if (isInverted)
                market = new MarketToProcess(ordered.Item1, ordered.Item2, candlesInTimeframe, startBalance);
            else
                market = new MarketToProcess(ordered.Item2, ordered.Item1, candlesInTimeframe, startBalance);

            for (int i = 0; i < candles.Count - 1; i++)
            {
                Candle current = candles[i];
                Candle next = candles[i + 1];

                //fake ticker
                Ticker t = new Ticker()
                {
                    last = next.High
                };

                breakoutStrategy.ProcessBuySell(market, t);

                //update market
                market.PushCandle(current);
            }

            log.Debug(" ------------------------------------------------------------------------------------------- ");
        }
    }

    public class ConsoleLogger : ILog
    {
        public string path;

        public ConsoleLogger(string path)
        {
            this.path = path;
        }

        public bool IsDebugEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ILogger Logger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        ILogger ILoggerWrapper.Logger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Debug(object message)
        {
            if (path != null)
            {
                string msg = (string)message;
                List<string> lines = new List<string>() { msg };
                File.AppendAllLines(path, lines);
            }
            else
                Console.WriteLine((string)message);
        }

        public void Debug(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Error(object message)
        {
            throw new NotImplementedException();
        }

        public void Error(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Info(object message)
        {
            string msg = "";
            List<string> lines = new List<string>() { msg };
            File.AppendAllLines(path, lines);
        }

        public void Info(object message, Exception exception)
        {
            Console.WriteLine((string)message);
        }

        public void InfoFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }
    }
}