using System;
namespace GeneticTree
{
    public class TradingSymbols
    {
        public static string[] OandaFXSymbols = {
            "AUDCAD","AUDCHF","AUDHKD","AUDJPY","AUDNZD","AUDSGD","AUDUSD","CADCHF","CADHKD","CADJPY",
            "CADSGD","CHFHKD","CHFJPY","CHFZAR","EURAUD","EURCAD","EURCHF","EURCZK","EURDKK","EURGBP",
            "EURHKD","EURHUF","EURJPY","EURNOK","EURNZD","EURPLN","EURSEK","EURSGD","EURTRY","EURUSD",
            "EURZAR","GBPAUD","GBPCAD","GBPCHF","GBPHKD","GBPJPY","GBPNZD","GBPPLN","GBPSGD","GBPUSD",
            "GBPZAR","HKDJPY","NZDCAD","NZDCHF","NZDHKD","NZDJPY","NZDSGD","NZDUSD","SGDCHF","SGDHKD",
            "SGDJPY","TRYJPY","USDCAD","USDCHF","USDCNH","USDCZK","USDDKK","USDHKD","USDHUF","USDINR",
            "USDJPY","USDMXN","USDNOK","USDPLN","USDSAR","USDSEK","USDSGD","USDTHB","USDTRY","USDZAR",
            "ZARJPY" };

        public static string[] OandaFXMajors2 = {
            "AUDJPY","AUDUSD","EURAUD","EURCHF","EURGBP","EURJPY","EURUSD","GBPCHF","GBPJPY","GBPUSD",
            "NZDUSD","USDCAD","USDCHF","USDJPY" };

        public static string[] OandaFXMajors1 = {
            "EURCHF","EURGBP","EURJPY","EURUSD","GBPCHF","GBPJPY","GBPUSD","USDCHF","USDJPY" };
        public static string[] OandaFXMajors = {
            "EURCHF","EURGBP","EURJPY","EURUSD" };
        public static string[] OandaCFD = {
            "EU50EUR", "FR40EUR", "DE30EUR" };

        public static string[] OandaFXMajors0 = {
            "EURUSD" };
    }
}
