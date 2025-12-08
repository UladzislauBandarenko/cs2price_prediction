using System.Globalization;
using System.Threading;
using cs2price_prediction.DTOs.AI.CaseHardenedKnife;
using cs2price_prediction.DTOs.AI.ChGuns;
using cs2price_prediction.DTOs.AI.Doppler;
using cs2price_prediction.DTOs.AI.FadeGun;
using cs2price_prediction.DTOs.AI.FadeKnife;
using cs2price_prediction.DTOs.AI.FloatGuns;
using cs2price_prediction.Services.AI.AiPromptService;
using FluentAssertions;
using Xunit;

namespace cs2price_prediction.Tests.Services.AI.AiPromptService
{
    public class AiPromptFactoryTests
    {
        private readonly AiPromptFactory _factory;

        public AiPromptFactoryTests()
        {
            // Чтобы в тестах всегда был разделитель "." (0.1999, 123.45 и т.д.)
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _factory = new AiPromptFactory();
        }

        [Fact]
        public void BuildCaseHardenedKnifePrompt_Builds_Expected_Text()
        {
            var r = new AiCaseHardenedKnifeRequest
            {
                Weapon = "Bayonet",
                Skin = "Case Hardened",
                Wear = "Factory New",
                Float = 0.0123,
                Pattern = 777,
                Stattrak = 1,
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60,
                PredictedPrice = 123.45
            };

            var prompt = _factory.BuildCaseHardenedKnifePrompt(r);

            prompt.Should().Contain("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Case Hardened knife.");
            prompt.Should().Contain("Answer in simple English, as continuous plain text, without lists, bullet points, headings or code.");
            prompt.Should().Contain("Use only the data provided below.");

            prompt.Should().Contain("Weapon: Bayonet");
            prompt.Should().Contain("Skin: Case Hardened");
            prompt.Should().Contain("Wear tier: Factory New");
            prompt.Should().Contain("Float value: 0.0123");
            prompt.Should().Contain("Pattern: 777");
            prompt.Should().Contain("StatTrak: yes");

            prompt.Should().Contain("Backside blue: 10%");
            prompt.Should().Contain("Backside purple: 20%");
            prompt.Should().Contain("Backside gold: 30%");
            prompt.Should().Contain("Playside blue: 40%");
            prompt.Should().Contain("Playside purple: 50%");
            prompt.Should().Contain("Playside gold: 60%");

            prompt.Should().Contain("Model predicted price: 123.45 USDT.");
            prompt.Should().Contain("Do not mention stickers at all, because this item should not have stickers.");
            prompt.Should().NotContain("Sticker slots and their price contribution");
        }

        [Fact]
        public void BuildChGunsPrompt_With_Stickers_Prints_Slots_And_StickerFooter()
        {
            var r = new AiChGunsRequest
            {
                Weapon = "AK-47",
                Skin = "Case Hardened",
                Wear = "Field-Tested",
                Float = 0.2345,
                Pattern = 123,
                Stattrak = 0,
                BacksideBlue = 30,
                PlaysideBlue = 40,
                BlueScore = 70,
                BlueTier = 3,
                Slot0Price = 10.50,
                Slot1Price = 0.0,
                Slot2Price = 3.33,
                Slot3Price = 0.0,
                StickerSlot1Name = "Sticker A",
                StickerSlot2Name = null,
                StickerSlot3Name = "Sticker C",
                StickerSlot4Name = "",
                PredictedPrice = 456.78
            };

            var prompt = _factory.BuildChGunsPrompt(r);

            prompt.Should().Contain("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Case Hardened gun.");
            prompt.Should().Contain("Weapon: AK-47");
            prompt.Should().Contain("Skin: Case Hardened");
            prompt.Should().Contain("Wear tier: Field-Tested");
            prompt.Should().Contain("Float value: 0.2345");
            prompt.Should().Contain("Pattern: 123");
            prompt.Should().Contain("StatTrak: no");

            prompt.Should().Contain("Backside blue: 30%");
            prompt.Should().Contain("Playside blue: 40%");
            prompt.Should().Contain("Blue score: 70, blue tier: 3");

            prompt.Should().Contain("Sticker slots and their price contribution (if any):");
            prompt.Should().Contain("Slot0: 10.50 USDT, name: Sticker A");
            prompt.Should().Contain("Slot1: 0.00 USDT, name: none");
            prompt.Should().Contain("Slot2: 3.33 USDT, name: Sticker C");
            prompt.Should().Contain("Slot3: 0.00 USDT, name: none");

            prompt.Should().Contain("Model predicted price: 456.78 USDT.");
            prompt.Should().Contain("and how the presence or absence of stickers and their total value influence the price.");
        }

        [Fact]
        public void BuildFloatGunsPrompt_Without_Stickers_Prints_NoMeaningfulStickers_Text()
        {
            var r = new AiFloatSensitiveGunsRequest
            {
                Weapon = "AWP",
                Skin = "Graphite",
                Wear = "Field-Tested",
                Float = 0.1999,
                Stattrak = 0,
                Slot0Price = 0,
                Slot1Price = 0,
                Slot2Price = 0,
                Slot3Price = 0,
                StickerSlot1Name = null,
                StickerSlot2Name = null,
                StickerSlot3Name = null,
                StickerSlot4Name = null,
                PredictedPrice = 321.0
            };

            var prompt = _factory.BuildFloatGunsPrompt(r);

            prompt.Should().Contain("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a float-sensitive gun.");
            prompt.Should().Contain("Weapon: AWP");
            prompt.Should().Contain("Skin: Graphite");
            prompt.Should().Contain("Wear tier: Field-Tested");
            prompt.Should().Contain("Float value: 0.1999");
            prompt.Should().NotContain("Pattern:");

            prompt.Should().Contain("StatTrak: no");

            // Ветка allZero && allNamesEmpty
            prompt.Should().Contain("There are no meaningful stickers on this weapon. Stickers should not influence the predicted price.");
            prompt.Should().NotContain("Sticker slots and their price contribution (if any):");

            prompt.Should().Contain("Model predicted price: 321.00 USDT.");
        }

        [Fact]
        public void BuildFadeGunsPrompt_Includes_Fade_And_Stickers()
        {
            var r = new AiFadeGunsRequest
            {
                Weapon = "M4A1-S",
                Skin = "Fade",
                Wear = "Factory New",
                Float = 0.0001,
                Pattern = 1,
                Stattrak = 0,
                FadePercentage = 95.5,
                FadeRank = 1,
                Slot0Price = 5.0,
                Slot1Price = 10.0,
                Slot2Price = 0.0,
                Slot3Price = 0.0,
                StickerSlot1Name = "Sticker 1",
                StickerSlot2Name = "Sticker 2",
                StickerSlot3Name = null,
                StickerSlot4Name = null,
                PredictedPrice = 777.77
            };

            var prompt = _factory.BuildFadeGunsPrompt(r);

            prompt.Should().Contain("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Fade gun.");
            prompt.Should().Contain("Weapon: M4A1-S");
            prompt.Should().Contain("Skin: Fade");
            prompt.Should().Contain("Wear tier: Factory New");
            prompt.Should().Contain("Float value: 0.0001");
            prompt.Should().Contain("Pattern: 1");
            prompt.Should().Contain("StatTrak: no");

            prompt.Should().Contain("Fade percentage: 95.5");
            prompt.Should().Contain("Fade rank: 1");

            prompt.Should().Contain("Sticker slots and their price contribution (if any):");
            prompt.Should().Contain("Slot0: 5.00 USDT, name: Sticker 1");
            prompt.Should().Contain("Slot1: 10.00 USDT, name: Sticker 2");
            prompt.Should().Contain("Slot2: 0.00 USDT, name: none");
            prompt.Should().Contain("Slot3: 0.00 USDT, name: none");

            prompt.Should().Contain("Model predicted price: 777.77 USDT.");
        }

        [Fact]
        public void BuildFadeKnivesPrompt_No_Stickers_And_Fade_Info()
        {
            var r = new AiFadeKnivesRequest
            {
                Weapon = "Bayonet",
                Skin = "Fade",
                Wear = "Minimal Wear",
                Float = 0.0123,
                Pattern = 1337,
                Stattrak = 1,
                FadePercentage = 90.0,
                FadeRank = 2,
                PredictedPrice = 555.55
            };

            var prompt = _factory.BuildFadeKnivesPrompt(r);

            prompt.Should().Contain("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Fade knife.");
            prompt.Should().Contain("Weapon: Bayonet");
            prompt.Should().Contain("Skin: Fade");
            prompt.Should().Contain("Wear tier: Minimal Wear");
            prompt.Should().Contain("Float value: 0.0123");
            prompt.Should().Contain("Pattern: 1337");
            prompt.Should().Contain("StatTrak: yes");

            prompt.Should().Contain("Fade percentage: 90");
            prompt.Should().Contain("Fade rank: 2");

            prompt.Should().Contain("Model predicted price: 555.55 USDT.");
            prompt.Should().Contain("Do not mention stickers at all, because this item should not have stickers.");
            prompt.Should().NotContain("Sticker slots and their price contribution");
        }

        [Fact]
        public void BuildDopplerPrompt_Contains_Phase_And_No_Pattern_Line()
        {
            var r = new AiDopplerRequest
            {
                Weapon = "Karambit",
                Skin = "Doppler",
                Wear = "Minimal Wear",
                Float = 0.0456,
                Stattrak = 1,
                Phase = "Ruby",
                PredictedPrice = 999.99
            };

            var prompt = _factory.BuildDopplerPrompt(r);

            prompt.Should().Contain("You are a CS2 skin pricing assistant. Explain why the model predicted this price for a Doppler knife.");
            prompt.Should().Contain("Weapon: Karambit");
            prompt.Should().Contain("Skin: Doppler");
            prompt.Should().Contain("Wear tier: Minimal Wear");
            prompt.Should().Contain("Float value: 0.0456");
            prompt.Should().NotContain("Pattern:");
            prompt.Should().Contain("StatTrak: yes");

            prompt.Should().Contain("Doppler phase: Ruby");
            prompt.Should().Contain("Model predicted price: 999.99 USDT.");
            prompt.Should().Contain("Do not mention stickers at all, because this item should not have stickers.");
        }
    }
}
