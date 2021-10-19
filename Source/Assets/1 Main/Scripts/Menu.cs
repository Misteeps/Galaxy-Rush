using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace Game
{
    public class Menu : MonoBehaviour
    {
        public void Start()
        {
            Global.Initialize();
            Settings.Initialize();
            Input.Initialize();
            Transition.Initialize();

            Global.menu = this;
            Time.timeScale = 1;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;

            Initialize();
        }


        private const float sideMenuYStart = -405;
        private const float sideMenuYEnd = -385;

        #region Foreground
        [Serializable]
        public class Foreground : UI.Group
        {
            public override void Initialize()
            {
                group.alpha = 1;
                UI.EnableHitboxes(group, true);

                Hide(1f);
            }
            public override void Update()
            {

            }
        }
        #endregion Foreground
        #region Main
        [Serializable]
        public class MainMenu : UI.Group
        {
            public GameObject play;
            public GameObject chapters;
            public GameObject settings;
            public GameObject credits;

            public GameObject effect;
            public GameObject effectTarget;

            public override void Initialize()
            {
                group.alpha = 1;
                UI.EnableHitboxes(group, true);
            }
            public override void Update()
            {
                GameObject hovered = Global.menu.cursor.hovered;

                if (effectTarget != hovered)
                    if (hovered == play || hovered == chapters || hovered == settings || hovered == credits)
                        MoveButton(hovered);
                    else
                        MoveButton(null);
            }

            public void MoveButton(GameObject target)
            {
                if (target == null)
                {
                    Transition.Add(effect, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Quartic, EaseDirections.Out, 1, 0, 0.25f);
                    effectTarget = null;
                    return;
                }

                Transition.Add(effect, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Elastic, EaseDirections.Out, effect.transform.localPosition.y, target.transform.localPosition.y, 0.2f);
                Transition.Add(effect, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Quartic, EaseDirections.Out, 0, 1, 0.2f);
                effectTarget = target;
            }
        }
        #endregion Main
        #region Chapters
        [Serializable]
        public class ChaptersMenu : UI.Group
        {
            public GameObject close;
            public bool active;

            public override void Update()
            {
                if (!active) return;
                GameObject hovered = Global.menu.cursor.hovered;

                if (Input.click.Down && hovered != null)
                    if (hovered == close) Hide();
            }

            public override void Show(float speed = 0.25F)
            {
                base.Show(speed);

                group.transform.localPosition = new Vector3(group.transform.localPosition.x, sideMenuYStart, group.transform.localPosition.z);
                Transition.Add(group.gameObject, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Quadratic, EaseDirections.Out, sideMenuYStart, sideMenuYEnd, speed);

                active = true;
            }
            public override void Hide(float speed = 0.1F)
            {
                base.Hide(speed);

                active = false;
            }
        }
        #endregion Chapters
        #region Settings
        [Serializable]
        public class SettingsMenu : UI.Group
        {
            private const int maxScrollHeight = 720;
            private const int scrollbarMax = -510;

            #region Slider Input
            [Serializable]
            public class SliderInput
            {
                public Transform root;
                private RectTransform gauge;
                private RectTransform fill;
                private RectTransform knob;
                private TMPro.TextMeshProUGUI textBox;

                private string[] labels;
                private string prefix;
                private float scale;
                private int value;
                private int min;
                private int max;

                private bool active;
                private bool delayed;
                private Action<float> onChange;


                public void Initialize(float value, float min, float max, Action<float> action) => Initialize(value, min, max, 1, string.Empty, null, false, action);
                public void Initialize(float value, float min, float max, string prefix, Action<float> action) => Initialize(value, min, max, 1, prefix, null, false, action);
                public void Initialize(float value, float min, float max, int scale, Action<float> action) => Initialize(value, min, max, scale, string.Empty, null, false, action);
                public void Initialize(float value, float min, float max, int scale, string prefix, string[] labels, bool delayed, Action<float> action)
                {
                    Transform slider = root.Find("Slider Input");
                    textBox = slider.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
                    gauge = slider.GetChild(1).GetComponent<RectTransform>();
                    fill = gauge.GetChild(0).GetComponent<RectTransform>();
                    knob = gauge.GetChild(1).GetComponent<RectTransform>();

                    this.labels = labels;
                    this.prefix = prefix;
                    this.scale = scale;
                    this.value = Mathf.RoundToInt(value * scale);
                    this.min = Mathf.RoundToInt(min * scale);
                    this.max = Mathf.RoundToInt(max * scale);
                    this.active = false;
                    this.delayed = delayed;

                    SetText();
                    SetSlider();

                    onChange = action;
                }
                public void Update()
                {
                    if (active)
                    {
                        if (!Input.click.Held)
                        {
                            CalculateSlider(true);
                            active = false;
                        }

                        CalculateSlider(false);
                    }
                    else if (Input.click.Down && Global.menu.cursor.hovered == gauge.gameObject) active = true;
                }

                public void SetText(string text) => textBox.text = text;
                public void SetText()
                {
                    if (labels == null) textBox.text = prefix + (value / scale).ToString();
                    else textBox.text = labels[Mathf.RoundToInt(value / scale)];
                }
                public void SetSlider()
                {
                    knob.anchoredPosition = new Vector3(FromValue(value), 0, 0);
                    knob.localPosition = new Vector3(knob.localPosition.x, 0, 0);
                    fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, knob.anchoredPosition.x);
                }
                public void CalculateSlider(bool forceOnChange)
                {
                    knob.position = new Vector2(Global.menu.cursor.group.transform.position.x, knob.position.y);
                    int value = FromPosition(knob.anchoredPosition.x);
                    SetSlider();

                    if (this.value != value)
                    {
                        this.value = value;
                        SetText();

                        if (!delayed) onChange.Invoke(value / scale);
                    }

                    if (forceOnChange) onChange.Invoke(value / scale);
                }

                public float FromValue(float value) => Mathf.Lerp(0, gauge.rect.size.x, Mathf.InverseLerp(min, max, value));
                public int FromPosition(float position) => Mathf.RoundToInt(Mathf.Lerp(min, max, Mathf.InverseLerp(0, gauge.rect.size.x, position)));
            }
            #endregion Slider Input
            #region Color Input
            [Serializable]
            public class ColorInput
            {
                public Transform root;
                private SliderInput red;
                private SliderInput green;
                private SliderInput blue;
                private SliderInput alpha;

                private Color color;

                private Action<Color> onChange;


                public void Initialize(Color color, Action<Color> action)
                {
                    this.color = color;

                    red = new SliderInput { root = root.Find("Color R Input") };
                    green = new SliderInput { root = root.Find("Color G Input") };
                    blue = new SliderInput { root = root.Find("Color B Input") };
                    alpha = new SliderInput { root = root.Find("Color A Input") };

                    red.Initialize(GetInt(color.r), 0, 255, "R: ", EditColorR);
                    green.Initialize(GetInt(color.g), 0, 255, "G: ", EditColorG);
                    blue.Initialize(GetInt(color.b), 0, 255, "B: ", EditColorB);
                    alpha.Initialize(GetInt(color.a), 0, 255, "A: ", EditColorA);

                    onChange = action;
                }
                public void Update()
                {
                    red.Update();
                    green.Update();
                    blue.Update();
                    alpha.Update();
                }

                public void EditColorR(float value) => EditColor(new Color(GetFloat(value), color.g, color.b, color.a));
                public void EditColorG(float value) => EditColor(new Color(color.r, GetFloat(value), color.b, color.a));
                public void EditColorB(float value) => EditColor(new Color(color.r, color.g, GetFloat(value), color.a));
                public void EditColorA(float value) => EditColor(new Color(color.r, color.g, color.b, GetFloat(value)));
                public void EditColor(Color color)
                {
                    if (this.color == color) return;
                    this.color = color;

                    onChange.Invoke(color);
                }

                public float GetFloat(float value) => Mathf.InverseLerp(0, 255, Mathf.RoundToInt(value));
                public float GetInt(float value) => Mathf.RoundToInt(Mathf.Lerp(0, 255, value));
            }
            #endregion Color Input
            #region Keybind Input
            [Serializable]
            public class KeybindInput
            {
                public Transform root;
                private GameObject primary;
                private GameObject secondary;
                private TMPro.TextMeshProUGUI primaryText;
                private TMPro.TextMeshProUGUI secondaryText;

                private Input.Trigger trigger;
                private bool primaryEditable;
                private bool secondaryEditable;

                private int active;
                //private Action onChange;
                // UI.EnableHitboxes(!value);
                // active = !value;


                public void Initialize(Input.Trigger trigger, bool primaryEditable = true, bool secondaryEditable = true)
                {
                    primary = root.GetChild(2).gameObject;
                    secondary = root.GetChild(3).gameObject;
                    primaryText = primary.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
                    secondaryText = secondary.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();

                    this.trigger = trigger;
                    this.primaryEditable = primaryEditable;
                    this.secondaryEditable = secondaryEditable;
                    this.active = 0;

                    Refresh();
                }
                public void Refresh()
                {
                    primaryText.text = Input.KeycodeString(trigger.primary);
                    secondaryText.text = Input.KeycodeString(trigger.secondary);
                }
                public void Update()
                {
                    if (active == 0 && Input.click.Down)
                    {
                        if (Global.menu.cursor.hovered == primary) Focus(1);
                        else if (Global.menu.cursor.hovered == secondary) Focus(2);
                    }
                    else
                        foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
                            if (UnityEngine.Input.GetKeyDown(keycode))
                                Edit(keycode);
                }

                public void Focus(int active)
                {
                    if (this.active == active) return;

                    if (active == 0)
                    {
                        Highlight(primary, false);
                        Highlight(secondary, false);
                        Refresh();

                        LockSettings(false);
                    }
                    else if (active == 1)
                    {
                        if (!primaryEditable) { Error(primary); return; }

                        Highlight(primary, true);
                        primaryText.text = "Editing Keybind";

                        LockSettings(true);
                    }
                    else if (active == 2)
                    {
                        if (!secondaryEditable) { Error(secondary); return; }

                        Highlight(secondary, true);
                        secondaryText.text = "Editing Keybind";

                        LockSettings(true);
                    }

                    this.active = active;
                }
                public void Edit(KeyCode keycode)
                {
                    if (keycode == KeyCode.Escape) keycode = KeyCode.None;

                    if (active == 1)
                        trigger.Set(keycode, null);
                    else if (active == 2)
                        trigger.Set(null, keycode);

                    Focus(0);
                }

                public void Highlight(GameObject button, bool active)
                {
                    if (active)
                    {
                        Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.R, EaseFunctions.Elastic, EaseDirections.Out, 1, 0, 0.3f);
                        Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.G, EaseFunctions.Elastic, EaseDirections.Out, 1, 0.5f, 0.3f);
                        Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.A, EaseFunctions.Elastic, EaseDirections.Out, 0.1f, 0.5f, 0.3f);
                    }
                    else
                    {
                        Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.R, EaseFunctions.Quartic, EaseDirections.Out, 0, 1, 0.2f);
                        Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.G, EaseFunctions.Quartic, EaseDirections.Out, 0.5f, 1, 0.2f);
                        Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.A, EaseFunctions.Quartic, EaseDirections.Out, 0.5f, 0.1f, 0.2f);
                    }
                }
                public void Error(GameObject button)
                {
                    button.GetComponent<Image>().color = new Color(1, 0, 0, 1);
                    Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.G, EaseFunctions.Quartic, EaseDirections.Out, 0, 1, 0.5f);
                    Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.B, EaseFunctions.Quartic, EaseDirections.Out, 0, 1, 0.5f);
                    Transition.Add(button, TransitionComponents.UIColor, TransitionUnits.A, EaseFunctions.Quartic, EaseDirections.Out, 0, 0.1f, 0.5f);
                }

                public async void LockSettings(bool locked)
                {
                    UI.EnableHitboxes(Global.menu.settings.group, !locked);
                    await System.Threading.Tasks.Task.Delay(1);
                    Global.menu.settingsLocked = locked;
                }
            }
            #endregion Keybind Input

            public GameObject items;
            public GameObject scrollbar;

            public SliderInput cursorSensitivity;
            public SliderInput lookSensitivity;

            public SliderInput cursorSize;
            public ColorInput cursorColor;
            public ColorInput cursorFocusedColor;

            public SliderInput fov;

            public SliderInput brightness;
            public SliderInput contrast;
            public SliderInput gamma;
            public SliderInput saturation;
            public SliderInput hueShift;
            public SliderInput bloom;
            public SliderInput chromaticAberration;

            public SliderInput sfxVolume;
            public SliderInput musicVolume;

            public KeybindInput click;
            public KeybindInput escape;
            public KeybindInput shoot;
            public KeybindInput aim;
            public KeybindInput speedUp;
            public KeybindInput slowDown;
            public KeybindInput strafeRight;
            public KeybindInput strafeLeft;
            public KeybindInput dash;
            public KeybindInput jump;

            public GameObject close;
            public bool active;

            public override void Initialize()
            {
                base.Initialize();

                cursorSensitivity.Initialize(Settings.cursorSensitivity, 1, 100, Settings.CursorSensitivity);
                lookSensitivity.Initialize(Settings.lookSensitivity, 1, 100, Settings.LookSensitivity);

                cursorSize.Initialize(Settings.cursorSize, 1, 100, Settings.CursorSize);
                cursorColor.Initialize(Settings.cursorColor, Settings.CursorColor);
                cursorFocusedColor.Initialize(Settings.cursorFocusedColor, Settings.CursorFocusedColor);

                fov.Initialize(Settings.fov, 60, 120, Settings.Fov);

                brightness.Initialize(Settings.brightness, 0.1f, 2, 100, Settings.Brightness);
                contrast.Initialize(Settings.contrast, -100, 100, Settings.Contrast);
                gamma.Initialize(Settings.gamma, 0.1f, 2, 100, Settings.Gamma);
                saturation.Initialize(Settings.saturation, -100, 100, Settings.Saturation);
                hueShift.Initialize(Settings.hueShift, -180, 180, Settings.HueShift);
                bloom.Initialize(Settings.bloom, 0, 20, Settings.Bloom);
                chromaticAberration.Initialize(Settings.chromaticAberration, 0, 1, 100, Settings.ChromaticAberration);

                sfxVolume.Initialize(Settings.sfxVolume, 0, 100, Settings.SFXVolume);
                musicVolume.Initialize(Settings.musicVolume, 0, 100, Settings.MusicVolume);

                click.Initialize(Input.click, false);
                escape.Initialize(Input.escape, false);
                shoot.Initialize(Input.shoot);
                aim.Initialize(Input.aim);
                speedUp.Initialize(Input.speedUp);
                slowDown.Initialize(Input.slowDown);
                strafeRight.Initialize(Input.strafeRight);
                strafeLeft.Initialize(Input.strafeLeft);
                dash.Initialize(Input.dash);
                jump.Initialize(Input.jump);
            }
            public override void Update()
            {
                if (!active) return;
                GameObject hovered = Global.menu.cursor.hovered;

                if (!Global.menu.settingsLocked)
                {
                    if (Input.click.Down && hovered != null)
                        if (hovered == close) Hide();

                    if (Input.scroll != 0)
                    {
                        float iStart = items.transform.localPosition.y;
                        float iEnd = Mathf.Clamp(items.transform.localPosition.y - Input.scroll * 2000, 285, maxScrollHeight + 285);

                        float sStart = scrollbar.transform.localPosition.y;
                        float sEnd = Mathf.Lerp(285, scrollbarMax + 285, Mathf.InverseLerp(285, maxScrollHeight + 285, iEnd));

                        Transition.Add(items, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Cubic, EaseDirections.Out, iStart, iEnd, 0.25f);
                        Transition.Add(scrollbar, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Cubic, EaseDirections.Out, sStart, sEnd, 0.25f);
                    }
                }

                cursorSensitivity.Update();
                lookSensitivity.Update();

                cursorSize.Update();
                cursorColor.Update();
                cursorFocusedColor.Update();

                fov.Update();

                brightness.Update();
                contrast.Update();
                gamma.Update();
                saturation.Update();
                hueShift.Update();
                bloom.Update();
                chromaticAberration.Update();

                sfxVolume.Update();
                musicVolume.Update();

                click.Update();
                escape.Update();
                shoot.Update();
                aim.Update();
                speedUp.Update();
                slowDown.Update();
                strafeRight.Update();
                strafeLeft.Update();
                dash.Update();
                jump.Update();
            }

            public override void Show(float speed = 0.25F)
            {
                base.Show(speed);

                group.transform.localPosition = new Vector3(group.transform.localPosition.x, sideMenuYStart, group.transform.localPosition.z);
                Transition.Add(group.gameObject, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Quadratic, EaseDirections.Out, sideMenuYStart, sideMenuYEnd, speed);

                active = true;
            }
            public override void Hide(float speed = 0.1F)
            {
                base.Hide(speed);

                active = false;
            }
        }
        #endregion Settings
        #region Credits
        [Serializable]
        public class CreditsMenu : UI.Group
        {
            public GameObject close;
            public bool active;

            public override void Update()
            {
                if (!active) return;
                GameObject hovered = Global.menu.cursor.hovered;

                if (Input.click.Down && hovered != null)
                    if (hovered == close) Hide();
            }

            public override void Show(float speed = 0.25F)
            {
                base.Show(speed);

                group.transform.localPosition = new Vector3(group.transform.localPosition.x, sideMenuYStart, group.transform.localPosition.z);
                Transition.Add(group.gameObject, TransitionComponents.LocalPosition, TransitionUnits.Y, EaseFunctions.Quadratic, EaseDirections.Out, sideMenuYStart, sideMenuYEnd, speed);

                active = true;
            }
            public override void Hide(float speed = 0.1F)
            {
                base.Hide(speed);

                active = false;
            }
        }
        #endregion Credits


        public UI.Cursor cursor;
        public Foreground foreground;
        public MainMenu main;
        public ChaptersMenu chapters;
        public SettingsMenu settings;
        public CreditsMenu credits;

        public Transform bounds;
        public Transform view;

        public bool settingsLocked;

        public void Initialize()
        {
            cursor.Initialize();
            foreground.Initialize();
            main.Initialize();
            chapters.Initialize();
            settings.Initialize();
            credits.Initialize();
        }
        public void Update()
        {
            Transition.IncrementObjects(Time.deltaTime);
            Transition.IncrementTweens(Time.deltaTime);

            cursor.Move(Input.mouseDelta, bounds.localPosition);
            cursor.GetHovered();

            if (settingsLocked)
            {
                settings.Update();
                return;
            }

            main.Update();
            chapters.Update();
            settings.Update();
            credits.Update();

            if (Input.click.Down && cursor.hovered != null)
            {
                if (cursor.hovered == main.play) Debug.Log("Play");
                else if (cursor.hovered == main.chapters) { CloseSideMenus(); chapters.Show(); }
                else if (cursor.hovered == main.settings) { CloseSideMenus(); settings.Show(); }
                else if (cursor.hovered == main.credits) { CloseSideMenus(); credits.Show(); }
            }

            if (Input.escape.Down)
                CloseSideMenus();
        }

        public void CloseSideMenus()
        {
            chapters.Hide();
            settings.Hide();
            credits.Hide();
        }
    }
}