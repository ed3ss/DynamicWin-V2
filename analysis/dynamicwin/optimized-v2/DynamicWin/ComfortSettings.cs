using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;

namespace DynamicWin;

[SettingsControl("local.comfort", "local.comfortcg")]
public sealed class ComfortSettings : ISettingsControlGroup
{
    private static void Save<T>(string key, T value)
    {
        Settings.SetSetting(key, value);
        ComfortRuntime.Load();
        Settings.SaveAllSettings();
    }

    private static void Toggle(List<UIObject> list, string key, string label, string description)
    {
        var toggle = new FRoundToggle();
        toggle.SilentSetSelected(Settings.GetSetting(key, true));
        toggle.OnSelectionChanged += (value, _) => Save(key, value);
        Settings.CreateSettingControlBase(list, toggle, label, description);
    }

    private static void Slider(List<UIObject> list, string key, string label, float min, float max, float initial, string description)
    {
        var slider = new FSlider(null, 150f);
        slider.MinValue.SetStaticState(min);
        slider.MaxValue.SetStaticState(max);
        slider.SilentSetValue(Settings.GetSetting(key, initial));
        slider.OnValueChanged += value => { Settings.SetSetting(key, Math.Clamp(value, min, max)); ComfortRuntime.Load(); };
        Settings.CreateSettingControlBase(list, slider, label, description);
    }

    List<UIObject> ISettingsControlGroup.Create()
    {
        var list = new List<UIObject>();
        Toggle(list, "comfort.game-mode", "Mode jeu automatique", "Masque l’îlot devant Fortnite et les applications en plein écran. Suspend les animations et le calcul du visualiseur.");
        Toggle(list, "comfort.economy", "Mode économie", "15 images/s après 10 s sans interaction et sans musique ; jusqu’à 60 images/s en activité.");
        var excluded = new FTextInputField(new FText(TextModelFactory.CreateBasic("")))
        {
            Text = Settings.GetSetting("comfort.exclusions", ""), PlaceholderText = "jeu.exe; logiciel.exe"
        };
        excluded.MaxCharacters.SetStaticState(1000);
        excluded.Transform.Size.SetStaticState(new Vector2(230, 32));
        Settings.CreateSettingControlBase(list, excluded, "Applications exclues", "Noms exacts des exécutables séparés par ; — masque l’îlot lorsqu’ils sont au premier plan.");
        var feedback = new FText(TextModelFactory.CreateBasic(""), size: () => new Vector2(350, 20));
        var saveExcluded = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Enregistrer")));
        saveExcluded.OnClick += () =>
        {
            var values = ComfortOptions.ParseExclusions(excluded.Text);
            string normalized = string.Join("; ", values);
            Save("comfort.exclusions", normalized);
            excluded.Text = normalized;
            feedback.Model = TextModelFactory.CreateBasic(values.Length + " exclusion(s) enregistrée(s).", 13);
        };
        Settings.CreateSettingControlBase(list, saveExcluded, "Appliquer les exclusions");
        list.Add(feedback);
        Settings.CreateSettingsControlSeparator(list);
        Settings.CreateSettingsControlTitle(list, "Apparence");
        Slider(list, "notch.width-finetune", "Largeur", 5f / 6f, 2.5f, 1, "Même réglage que dans la rubrique Notch.");
        Slider(list, "comfort.x", "Décalage horizontal", -500, 500, 0, "En pixels, autour de la position centrale.");
        Slider(list, "comfort.y", "Décalage vertical", 0, 250, 0, "En pixels depuis le haut de l’écran.");
        Slider(list, "comfort.opacity", "Opacité", .3f, 1, 1, "De 30 % à 100 %.");
        var color = new FTextInputField(new FText(TextModelFactory.CreateBasic("")))
        { Text = Settings.GetSetting("comfort.color", "#000000"), PlaceholderText = "#000000" };
        color.MaxCharacters.SetStaticState(7);
        color.Transform.Size.SetStaticState(new Vector2(120, 32));
        Settings.CreateSettingControlBase(list, color, "Couleur de fond", "Code hexadécimal #RRGGBB.");
        var applyColor = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Appliquer")));
        var colorStatus = new FText(TextModelFactory.CreateBasic(""), size: () => new Vector2(350, 20));
        applyColor.OnClick += () =>
        {
            string value = color.Text.Trim();
            if (!ComfortOptions.ValidColor(value)) { colorStatus.Model = TextModelFactory.CreateBasic("Format attendu : #RRGGBB", 13); return; }
            Save("comfort.color", value);
            colorStatus.Model = TextModelFactory.CreateBasic("Couleur appliquée", 13);
        };
        Settings.CreateSettingControlBase(list, applyColor, "Appliquer la couleur");
        list.Add(colorStatus);
        var screen = new FSlider(null, 150f);
        screen.MinValue.SetStaticState(0);
        screen.MaxValue.SetStaticState(Math.Max(1, System.Windows.Forms.Screen.AllScreens.Length - 1));
        screen.SnappingInterval = 1;
        screen.SilentSetValue(Settings.GetSetting("general.active-display", 0));
        screen.OnValueChanged += value => Save("general.active-display", Math.Clamp((int)Math.Round(value), 0, System.Windows.Forms.Screen.AllScreens.Length - 1));
        Settings.CreateSettingControlBase(list, screen, "Écran de l’îlot", "0 = premier écran, 1 = deuxième… Sélection visuelle également dans General.");
        Settings.CreateSettingsControlSeparator(list);
        Toggle(list, "comfort.shortcuts", "Raccourcis clavier", "Ctrl+Alt+D : masquer/afficher ; Ctrl+Alt+P : lecture/pause ; Ctrl+Alt+←/→ : précédent/suivant. Le masquage en jeu reste prioritaire.");
        Toggle(list, "comfort.diagnostics", "Journal local", "Deux fichiers de 256 Ko environ au maximum. Aucun titre musical ni identifiant Discord enregistré. Aucun envoi automatique.");
        var logs = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Ouvrir")));
        logs.OnClick += () =>
        {
            Directory.CreateDirectory(LocalDiagnostics.Folder);
            Process.Start(new ProcessStartInfo(LocalDiagnostics.Folder) { UseShellExecute = true });
        };
        Settings.CreateSettingControlBase(list, logs, "Dossier de diagnostic");
        var saveAll = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Enregistrer")));
        saveAll.OnClick += () => { Settings.SaveAllSettings(); feedback.Model = TextModelFactory.CreateBasic("Réglages enregistrés", 13); };
        Settings.CreateSettingControlBase(list, saveAll, "Sauvegarder les réglages", "Les curseurs sont aussi sauvegardés à la fermeture des paramètres.");
        return list;
    }
}
