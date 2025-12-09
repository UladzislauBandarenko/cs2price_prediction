using System.Text;
using cs2price_prediction.DTOs.AI.CaseHardenedKnife;
using cs2price_prediction.DTOs.AI.ChGuns;
using cs2price_prediction.DTOs.AI.Doppler;
using cs2price_prediction.DTOs.AI.FadeGun;
using cs2price_prediction.DTOs.AI.FadeKnife;
using cs2price_prediction.DTOs.AI.FloatGuns;

namespace cs2price_prediction.Services.AI.AiPromptService
{
    public class AiPromptFactory : IAiPromptFactory
    {
        // KNIFE: CASE HARDENED
        public string BuildCaseHardenedKnifePrompt(AiCaseHardenedKnifeRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Case Hardened knife.");
            CommonHeader(sb, r.Weapon, r.Skin, r.Wear, r.Float, r.Pattern, r.Stattrak == 1);
            sb.AppendLine($"Backside blue: {r.BacksideBlue:0.##}%");
            sb.AppendLine($"Backside purple: {r.BacksidePurple:0.##}%");
            sb.AppendLine($"Backside gold: {r.BacksideGold:0.##}%");
            sb.AppendLine($"Playside blue: {r.PlaysideBlue:0.##}%");
            sb.AppendLine($"Playside purple: {r.PlaysidePurple:0.##}%");
            sb.AppendLine($"Playside gold: {r.PlaysideGold:0.##}%");
            PriceFooterNoStickers(sb, r.PredictedPrice);
            return sb.ToString();
        }

        // GUN: CASE HARDENED
        public string BuildChGunsPrompt(AiChGunsRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Case Hardened gun.");
            CommonHeader(sb, r.Weapon, r.Skin, r.Wear, r.Float, r.Pattern, r.Stattrak == 1);
            sb.AppendLine($"Backside blue: {r.BacksideBlue:0.##}%");
            sb.AppendLine($"Playside blue: {r.PlaysideBlue:0.##}%");
            sb.AppendLine($"Blue score: {r.BlueScore:0.##}, blue tier: {r.BlueTier:0.##}");
            StickersBlock(
                sb,
                r.Slot0Price, r.Slot1Price, r.Slot2Price, r.Slot3Price,
                r.StickerSlot1Name, r.StickerSlot2Name, r.StickerSlot3Name, r.StickerSlot4Name
            );
            PriceFooterWithStickers(sb, r.PredictedPrice);
            return sb.ToString();
        }

        // KNIFE: DOPPLER
        public string BuildDopplerPrompt(AiDopplerRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Doppler knife.");
            CommonHeader(sb, r.Weapon, r.Skin, r.Wear, r.Float, pattern: null, stattrak: r.Stattrak == 1);
            sb.AppendLine($"Doppler phase: {r.Phase}");
            PriceFooterNoStickers(sb, r.PredictedPrice);
            return sb.ToString();
        }

        // GUN: FADE
        public string BuildFadeGunsPrompt(AiFadeGunsRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Fade gun.");
            CommonHeader(sb, r.Weapon, r.Skin, r.Wear, r.Float, r.Pattern, r.Stattrak == 1);
            sb.AppendLine($"Fade percentage: {r.FadePercentage:0.##}");
            sb.AppendLine($"Fade rank: {r.FadeRank:0.##}");
            StickersBlock(
                sb,
                r.Slot0Price, r.Slot1Price, r.Slot2Price, r.Slot3Price,
                r.StickerSlot1Name, r.StickerSlot2Name, r.StickerSlot3Name, r.StickerSlot4Name
            );
            PriceFooterWithStickers(sb, r.PredictedPrice);
            return sb.ToString();
        }

        // KNIFE: FADE
        public string BuildFadeKnivesPrompt(AiFadeKnivesRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Fade knife.");
            CommonHeader(sb, r.Weapon, r.Skin, r.Wear, r.Float, r.Pattern, r.Stattrak == 1);
            sb.AppendLine($"Fade percentage: {r.FadePercentage:0.##}");
            sb.AppendLine($"Fade rank: {r.FadeRank:0.##}");
            PriceFooterNoStickers(sb, r.PredictedPrice);
            return sb.ToString();
        }

        // GUN: FLOAT-SENSITIVE
        public string BuildFloatGunsPrompt(AiFloatSensitiveGunsRequest r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a float-sensitive gun.");
            CommonHeader(sb, r.Weapon, r.Skin, r.Wear, r.Float, pattern: null, stattrak: r.Stattrak == 1);
            StickersBlock(
                sb,
                r.Slot0Price, r.Slot1Price, r.Slot2Price, r.Slot3Price,
                r.StickerSlot1Name, r.StickerSlot2Name, r.StickerSlot3Name, r.StickerSlot4Name
            );
            PriceFooterWithStickers(sb, r.PredictedPrice);
            return sb.ToString();
        }

        // ================== COMMON HELPERS ==================

        private static void CommonHeader(
            StringBuilder sb,
            string weapon,
            string skin,
            string wear,
            double @float,
            int? pattern,
            bool stattrak)
        {
            sb.AppendLine("Answer in simple English, as continuous plain text, without lists, bullet points, headings or code.");
            sb.AppendLine("Use only the data provided below.");
            sb.AppendLine();
            sb.AppendLine($"Weapon: {weapon}");
            sb.AppendLine($"Skin: {skin}");
            sb.AppendLine($"Wear tier: {wear}");
            sb.AppendLine($"Float value: {@float:0.0000}");
            if (pattern.HasValue)
                sb.AppendLine($"Pattern: {pattern.Value}");
            sb.AppendLine($"StatTrak: {(stattrak ? "yes" : "no")}");
        }

        private static void StickersBlock(
            StringBuilder sb,
            double slot0,
            double slot1,
            double slot2,
            double slot3,
            string? name1,
            string? name2,
            string? name3,
            string? name4)
        {
            sb.AppendLine();

            bool allZero =
                slot0 <= 0 && slot1 <= 0 && slot2 <= 0 && slot3 <= 0;

            bool allNamesEmpty =
                string.IsNullOrWhiteSpace(name1) &&
                string.IsNullOrWhiteSpace(name2) &&
                string.IsNullOrWhiteSpace(name3) &&
                string.IsNullOrWhiteSpace(name4);

            if (allZero && allNamesEmpty)
            {
                sb.AppendLine("There are no meaningful stickers on this weapon. Stickers should not influence the predicted price.");
                return;
            }

            sb.AppendLine("Sticker slots and their price contribution (if any):");
            sb.AppendLine($"Slot0: {slot0:0.00} USDT, name: {(string.IsNullOrWhiteSpace(name1) ? "none" : name1)}");
            sb.AppendLine($"Slot1: {slot1:0.00} USDT, name: {(string.IsNullOrWhiteSpace(name2) ? "none" : name2)}");
            sb.AppendLine($"Slot2: {slot2:0.00} USDT, name: {(string.IsNullOrWhiteSpace(name3) ? "none" : name3)}");
            sb.AppendLine($"Slot3: {slot3:0.00} USDT, name: {(string.IsNullOrWhiteSpace(name4) ? "none" : name4)}");
        }

        private static void PriceFooterNoStickers(StringBuilder sb, double predicted)
        {
            sb.AppendLine();
            sb.AppendLine($"Model predicted price: {predicted:0.00} USDT.");
            sb.AppendLine("Explain which factors most strongly affect this price, focusing on float, wear, pattern or phase, color distribution and StatTrak. Do not mention stickers at all, because this item should not have stickers.");
        }

        private static void PriceFooterWithStickers(StringBuilder sb, double predicted)
        {
            sb.AppendLine();
            sb.AppendLine($"Model predicted price: {predicted:0.00} USDT.");
            sb.AppendLine("Explain which factors most strongly affect this price, including float, wear, pattern or fade characteristics, and how the presence or absence of stickers and their total value influence the price.");
        }
    }
}
