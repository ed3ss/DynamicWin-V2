using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;

namespace DynamicWin;

[SettingsControl("local.discord", "local.discordcg")]
public class DiscordSettings : ISettingsControlGroup
{
    internal const string EnabledKey = "discord.enabled";
    internal const string IdKey = "discord.application-id";

    internal static void Apply()
    {
        DiscordPresence.Instance.Configure(Settings.GetSetting(EnabledKey, false), Settings.GetSetting(IdKey, ""), Settings.GetSetting("discord.cover-art", true));
    }

    List<UIObject> ISettingsControlGroup.Create()
    {
        var list = new List<UIObject>();
        var toggle = new FRoundToggle();
        toggle.SilentSetSelected(Settings.GetSetting(EnabledKey, false));
        toggle.OnSelectionChanged += (selected, _) =>
        {
            if (selected && !DiscordActivity.ValidApplicationId(Settings.GetSetting(IdKey, "")))
            {
                toggle.SilentSetSelected(false);
                return;
            }
            Settings.SetSetting(EnabledKey, selected);
            Settings.SaveAllSettings();
            Apply();
        };
        Settings.CreateSettingControlBase(list, toggle, "Partager ma musique", "Titre et artiste pendant la lecture. La présence est retirée à la pause.");
        Settings.CreateSettingsControlSeparator(list);
        var covers = new FRoundToggle();
        covers.SilentSetSelected(Settings.GetSetting("discord.cover-art", true));
        covers.OnSelectionChanged += (selected, _) =>
        {
            Settings.SetSetting("discord.cover-art", selected);
            Settings.SaveAllSettings();
            Apply();
        };
        Settings.CreateSettingControlBase(list, covers, "Afficher la pochette", "Recherche publique Apple/Deezer avec le titre et l’artiste. Aucun fichier local envoyé.");
        Settings.CreateSettingsControlSeparator(list);
        var input = new FTextInputField(new FText(TextModelFactory.CreateBasic("")))
        {
            Text = Settings.GetSetting(IdKey, ""),
            PlaceholderText = "17 à 20 chiffres"
        };
        input.MaxCharacters.SetStaticState(20);
        input.Transform.Size.SetStaticState(new Vector2(190, 30));
        Settings.CreateSettingControlBase(list, input, "Application ID");
        var message = new DiscordStatusText();
        var save = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Enregistrer")));
        save.OnClick += () =>
        {
            string id = input.Text.Trim();
            if (!DiscordActivity.ValidApplicationId(id))
            {
                message.ValidationMessage = "Identifiant invalide : 17 à 20 chiffres.";
                return;
            }
            message.ValidationMessage = null;
            Settings.SetSetting(IdKey, id);
            Settings.SaveAllSettings();
            Apply();
        };
        Settings.CreateSettingControlBase(list, save, "Connexion locale", "Discord doit être ouvert. Aucun token personnel nécessaire.");
        list.Add(message);
        return list;
    }

    private sealed class DiscordStatusText : FText
    {
        internal string? ValidationMessage;
        private string? previous;
        internal DiscordStatusText() : base(TextModelFactory.CreateBasic(""), size: () => new Vector2(350, 55))
        {
            Layout.StretchHorizontal.SetStaticState(true);
        }
        protected override void Update()
        {
            base.Update();
            string current = ValidationMessage ?? DiscordPresence.Instance.Status;
            if (current == previous) return;
            previous = current;
            Model = TextModelFactory.CreateBasic(current, 13);
        }
    }
}
