using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace GalaxyRush
{
    #region Global
    public static class Global
    {
        private static bool initialized;

        public static Dictionary<int, int> scores;

        public static Menu menu;
        public static Game game;
        public static SceneLoader loader;
        public static Camera camera;
        public static Cinemachine.CinemachineVirtualCamera cinemachine;
        public static Player player;


        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            scores = new Dictionary<int, int>();
        }

        public static string LevelName(int level)
        {
            switch (level)
            {
                default: return "Menu";
                case 0: return "Tutorial";
                case 1: return "1";
            }
        }
        public static string GetUnplayedLevel()
        {
            for (int i = 0; i < 10; i++)
                if (!scores.ContainsKey(i))
                    return LevelName(i);

            return "1";
        }

        public static void SetFOV() => SetFOV(Settings.fov);
        public static void SetFOV(float value) => cinemachine.m_Lens.FieldOfView = value;

        public static void NewScore(int level, int score)
        {
            if (!scores.ContainsKey(level))
                scores.Add(level, score);
            else if (scores[level] < score)
                scores[level] = score;

        }
    }
    #endregion Global

    #region Settings
    public static class Settings
    {
        private static bool initialized;

        public static float cursorSensitivity;
        public static float lookSensitivity;

        public static float cursorSize;
        public static Color cursorColor;
        public static Color cursorFocusedColor;

        public static float fov;

        public static float brightness;
        public static float contrast;
        public static float gamma;
        public static float saturation;
        public static float hueShift;
        public static float bloom;
        public static float chromaticAberration;

        public static float sfxVolume;
        public static float musicVolume;


        public static void Initialize()
        {
            if (!initialized)
            {
                Defaults();
                initialized = true;
            }

            CursorSensitivity(cursorSensitivity);
            LookSensitivity(lookSensitivity);

            CursorSize(cursorSize);
            CursorColor(cursorColor);
            CursorFocusedColor(cursorFocusedColor);

            Fov(fov);

            Brightness(brightness);
            Contrast(contrast);
            Gamma(gamma);
            Saturation(saturation);
            HueShift(hueShift);
            Bloom(bloom);
            ChromaticAberration(chromaticAberration);

            SFXVolume(sfxVolume);
            MusicVolume(musicVolume);
        }
        public static void Defaults()
        {
            CursorSensitivity(8);
            LookSensitivity(8);

            CursorSize(10);
            CursorColor(new Color(1, 1, 1, 0.5f));
            CursorFocusedColor(new Color(0, 1, 1, 1));

            Fov(80);

            Brightness(0);
            Contrast(0);
            Gamma(0);
            Saturation(0);
            HueShift(0);
            Bloom(4);
            ChromaticAberration(0);

            SFXVolume(50);
            MusicVolume(50);

#if UNITY_EDITOR
            CursorSensitivity(0.8f);
            LookSensitivity(0.6f);
            Fov(100);
#endif
        }

        public static void CursorSensitivity(float value)
        {
            cursorSensitivity = value;
        }
        public static void LookSensitivity(float value)
        {
            lookSensitivity = value;
        }

        public static void CursorSize(float value)
        {
            cursorSize = value;

            Global.menu?.cursor.SetSize(value);
            Global.game?.cursor.SetSize(value);
        }
        public static void CursorColor(Color value)
        {
            cursorColor = value;

            Global.menu?.cursor.SetColor(value, 0);
            Global.game?.cursor.SetColor(value, 0);
        }
        public static void CursorFocusedColor(Color value)
        {
            cursorFocusedColor = value;

            Global.menu?.cursor.SetColor(value, 0);
            Global.game?.cursor.SetColor(value, 0);
        }

        public static void Fov(float value)
        {
            fov = value;

            Global.SetFOV(value);
        }

        public static void Brightness(float value)
        {
            brightness = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out LiftGammaGain component);
            component.gain.value = Vector4.one * value;
        }
        public static void Contrast(float value)
        {
            contrast = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out ColorAdjustments component);
            component.contrast.value = value;
        }
        public static void Gamma(float value)
        {
            gamma = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out LiftGammaGain component);
            component.gamma.value = Vector4.one * value;
        }
        public static void Saturation(float value)
        {
            saturation = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out ColorAdjustments component);
            component.saturation.value = value;
        }
        public static void HueShift(float value)
        {
            hueShift = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out ColorAdjustments component);
            component.hueShift.value = value;
        }
        public static void Bloom(float value)
        {
            bloom = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out Bloom component);
            component.intensity.value = value;
        }
        public static void ChromaticAberration(float value)
        {
            chromaticAberration = value;

            Global.camera.GetComponent<Volume>().profile.TryGet(out ChromaticAberration component);
            component.intensity.value = value;
        }

        public static void SFXVolume(float value)
        {
            sfxVolume = value;

            Global.menu?.audioMixer.SetFloat("SFX", GetDecibel(value));
            Global.game?.audioMixer.SetFloat("SFX", GetDecibel(value));
        }
        public static void MusicVolume(float value)
        {
            musicVolume = value;

            Global.menu?.audioMixer.SetFloat("Music", GetDecibel(value));
            Global.game?.audioMixer.SetFloat("Music", GetDecibel(value));
        }

        public static float GetDecibel(float value)
        {
            if (value == 0) return -144;

            value = Mathf.InverseLerp(0, 100, value);
            return 20.0f * Mathf.Log10(value);
        }
    }
    #endregion Settings
    #region Input
    public static class Input
    {
        #region Trigger
        public class Trigger
        {
            public KeyCode primary;
            public KeyCode secondary;

            public bool Held => UnityEngine.Input.GetKey(primary) || UnityEngine.Input.GetKey(secondary);
            public bool Down => UnityEngine.Input.GetKeyDown(primary) || UnityEngine.Input.GetKeyDown(secondary);
            public bool Up => UnityEngine.Input.GetKeyUp(primary) || UnityEngine.Input.GetKeyUp(secondary);


            public Trigger(KeyCode primary, KeyCode secondary) => Set(primary, secondary);
            public void Set(KeyCode? primary = null, KeyCode? secondary = null)
            {
                if (primary != null) this.primary = primary.Value;
                if (secondary != null) this.secondary = secondary.Value;
            }
        }
        #endregion Trigger

        private static bool initialized;

        //public static Vector2 cursor => Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        //public static Vector2 cursor => Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
        public static Vector2 mouseDelta => new Vector2(UnityEngine.Input.GetAxis("X"), UnityEngine.Input.GetAxis("Y"));
        public static float scroll => UnityEngine.Input.GetAxis("Scroll");

        public static Trigger click;
        public static Trigger escape;

        public static Trigger shoot;
        public static Trigger aim;

        public static Trigger speedUp;
        public static Trigger slowDown;
        public static Trigger strafeRight;
        public static Trigger strafeLeft;

        public static Trigger dash;
        public static Trigger jump;


        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            click = new Trigger(KeyCode.Mouse0, KeyCode.None);
            escape = new Trigger(KeyCode.Escape, KeyCode.Mouse3);

            shoot = new Trigger(KeyCode.Mouse0, KeyCode.None);
            aim = new Trigger(KeyCode.Mouse1, KeyCode.None);

            speedUp = new Trigger(KeyCode.W, KeyCode.UpArrow);
            slowDown = new Trigger(KeyCode.S, KeyCode.DownArrow);
            strafeRight = new Trigger(KeyCode.D, KeyCode.RightArrow);
            strafeLeft = new Trigger(KeyCode.A, KeyCode.LeftArrow);

            dash = new Trigger(KeyCode.LeftShift, KeyCode.None);
            jump = new Trigger(KeyCode.Space, KeyCode.None);
        }

        public static string KeycodeString(KeyCode keycode)
        {
            switch (keycode)
            {
                default: return Regex.Replace(keycode.ToString(), "([a-z0-9])([A-Z0-9])", "$1 $2");
                case KeyCode.None: return string.Empty;
                case (KeyCode)541: return "Scroll Up";
                case (KeyCode)542: return "Scroll Down";
                case KeyCode.Mouse0: return "Left Click";
                case KeyCode.Mouse1: return "Right Click";
                case KeyCode.Mouse2: return "Scroll Click";
                case KeyCode.Mouse3: return "Mouse Thumb 1";
                case KeyCode.Mouse4: return "Mouse Thumb 2";
                case KeyCode.Return: return "Enter";
                case KeyCode.Alpha0: return "0";
                case KeyCode.Alpha1: return "1";
                case KeyCode.Alpha2: return "2";
                case KeyCode.Alpha3: return "3";
                case KeyCode.Alpha4: return "4";
                case KeyCode.Alpha5: return "5";
                case KeyCode.Alpha6: return "6";
                case KeyCode.Alpha7: return "7";
                case KeyCode.Alpha8: return "8";
                case KeyCode.Alpha9: return "9";
                case KeyCode.BackQuote: return "`";
                case KeyCode.Minus: return "-";
                case KeyCode.Equals: return "=";
                case KeyCode.LeftBracket: return "[";
                case KeyCode.RightBracket: return "]";
                case KeyCode.Backslash: return "\\";
                case KeyCode.Semicolon: return ";";
                case KeyCode.Quote: return "'";
                case KeyCode.Comma: return ",";
                case KeyCode.Period: return ".";
                case KeyCode.Slash: return "/";
            }
        }
    }
    #endregion Input

    #region UI
    public static class UI
    {
        #region Group
        [Serializable]
        public abstract class Group
        {
            public CanvasGroup group;

            public virtual void Initialize()
            {
                group.alpha = 0;
                EnableHitboxes(group, false);
            }
            public abstract void Update();

            public virtual void Show(float speed = 0.25f)
            {
                Transition.Add(group.gameObject, TransitionComponents.UIAlpha, TransitionUnits.A, EaseFunctions.Linear, EaseDirections.InOut, 0, 1, speed, true);
                EnableHitboxes(group, true);
            }
            public virtual void Hide(float speed = 0.1f)
            {
                Transition.Add(group.gameObject, TransitionComponents.UIAlpha, TransitionUnits.A, EaseFunctions.Linear, EaseDirections.InOut, 1, 0, speed, true);
                EnableHitboxes(group, false);
            }
        }
        #endregion Group
        #region Cursor
        [Serializable]
        public class Cursor : Group
        {
            public Image crosshair;
            public Image dot;

            public GameObject hovered;
            public Vector3 targeted;


            public override void Initialize()
            {
                group.alpha = 1;
                EnableHitboxes(group, false);
            }
            public override void Update() { }

            public void Move(Vector2 delta, Vector2 bounds)
            {
                float x = Mathf.Clamp(group.transform.localPosition.x + (delta.x * Settings.cursorSensitivity), -bounds.x, bounds.x);
                float y = Mathf.Clamp(group.transform.localPosition.y + (delta.y * Settings.cursorSensitivity), -bounds.y, bounds.y);

                group.transform.localPosition = new Vector3(x, y, 0);
            }
            public void Focus(bool focused)
            {
                if (focused)
                {
                    Transition.Add(crosshair.gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Cubic, EaseDirections.Out, 2f, 2.4f, 0.2f, true);
                    Transition.Add(crosshair.gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Cubic, EaseDirections.Out, 2f, 2.4f, 0.2f, true);

                    Transition.Add(dot.gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Exponential, EaseDirections.Out, 0f, 0.25f, 0.15f, true);
                    Transition.Add(dot.gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Exponential, EaseDirections.Out, 0f, 0.25f, 0.15f, true);

                    SetColor(Settings.cursorFocusedColor, 0.1f);
                }
                else
                {
                    Transition.Add(crosshair.gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Cubic, EaseDirections.Out, 2.4f, 2f, 0.2f, true);
                    Transition.Add(crosshair.gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Cubic, EaseDirections.Out, 2.4f, 2f, 0.2f, true);

                    Transition.Add(dot.gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Exponential, EaseDirections.Out, 0.25f, 0f, 0.15f, true);
                    Transition.Add(dot.gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Exponential, EaseDirections.Out, 0.25f, 0f, 0.15f, true);

                    SetColor(Settings.cursorColor, 0.1f);
                }
            }

            public void SetSize(float size) => group.transform.localScale = new Vector3(size, size, 1);
            public void SetColor(Color? color = null, float speed = 0)
            {
                if (color == null) color = (hovered == null) ? Settings.cursorColor : Settings.cursorFocusedColor;

                Transition.Add(crosshair.gameObject, TransitionComponents.UIColor, TransitionUnits.R, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.r, speed, true);
                Transition.Add(crosshair.gameObject, TransitionComponents.UIColor, TransitionUnits.G, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.g, speed, true);
                Transition.Add(crosshair.gameObject, TransitionComponents.UIColor, TransitionUnits.B, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.b, speed, true);
                Transition.Add(crosshair.gameObject, TransitionComponents.UIColor, TransitionUnits.A, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.a, speed, true);

                Transition.Add(dot.gameObject, TransitionComponents.UIColor, TransitionUnits.R, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.r, speed, true);
                Transition.Add(dot.gameObject, TransitionComponents.UIColor, TransitionUnits.G, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.g, speed, true);
                Transition.Add(dot.gameObject, TransitionComponents.UIColor, TransitionUnits.B, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.b, speed, true);
                Transition.Add(dot.gameObject, TransitionComponents.UIColor, TransitionUnits.A, EaseFunctions.Linear, EaseDirections.InOut, 0, color.Value.a, speed, true);
            }

            public void GetHovered()
            {
                RaycastHit2D hit = Physics2D.Raycast(group.transform.position, Vector2.zero);
                if (hit.collider?.gameObject == this.hovered) return;

                this.hovered = hit.collider?.gameObject;
                Focus(this.hovered != null);
            }
            public void GetTargeted()
            {
                Ray ray = new Ray(Global.player.cameraPosition.position, Global.player.cameraPosition.transform.TransformDirection(Vector3.forward));
                if (Physics.Raycast(ray, out RaycastHit hit, 100, Global.player.shots.collidableLayers))
                {
                    targeted = hit.point;
                    Focus(Global.player.shots.targetLayers == (Global.player.shots.targetLayers | (1 << hit.collider.gameObject.layer)));
                }
                else
                {
                    targeted = ray.GetPoint(100);
                    Focus(false);
                }
            }
        }
        #endregion Cursor

        public static void ColorObject(GameObject gameObject, float? r = null, float? g = null, float? b = null, float? a = null) => ColorObject(gameObject.GetComponent<Image>(), r.Value, g.Value, b.Value, a.Value);
        public static void ColorObject(GameObject gameObject, Color color) => ColorObject(gameObject.GetComponent<Image>(), color);
        public static void ColorObject(Image image, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            if (r == null) r = image.color.r;
            if (g == null) g = image.color.g;
            if (b == null) b = image.color.b;
            if (a == null) a = image.color.a;

            ColorObject(image, new Color(r.Value, g.Value, b.Value, a.Value));
        }
        public static void ColorObject(Image image, Color color) => image.color = color;

        public static void EnableHitboxes(CanvasGroup group, bool enabled) => EnableHitboxes(group.gameObject, enabled);
        public static void EnableHitboxes(GameObject gameObject, bool enabled)
        {
            foreach (Collider2D collider in gameObject.GetComponentsInChildren<Collider2D>())
                collider.enabled = enabled;
        }
    }
    #endregion UI

    #region Transition
    // DO NOT INSERT COMPONENTS. APPEND ONLY.
    public enum TransitionComponents { Position, LocalPosition, Rotation, Scale, UIColor, UIAlpha }
    public enum TransitionUnits { X, Y, Z, W, R, G, B, A }
    public enum EaseFunctions { Linear, Quadratic, Cubic, Quartic, Quintic, Sine, Circular, Exponential, Elastic, Back, Bounce }
    public enum EaseDirections { In, Out, InOut }


    public static class Transition
    {
        #region Object
        public class Object
        {
            public GameObject gameObject;
            public Transform transform;
            public Image image;
            public CanvasGroup canvasGroup;
            public Dictionary<(TransitionComponents component, TransitionUnits unit), Tween> tweens;

            public bool ignoreTimeScale;


            public Object(GameObject gameObject)
            {
                this.gameObject = gameObject;
                transform = gameObject.GetComponent<Transform>();
                image = gameObject.GetComponent<Image>();
                canvasGroup = gameObject.GetComponent<CanvasGroup>();
                tweens = new Dictionary<(TransitionComponents component, TransitionUnits unit), Tween>();
            }

            public void Set(TransitionComponents component, TransitionUnits unit, EaseFunctions function, EaseDirections direction, float start, float end, float duration)
            {
                Tween tween = new Tween(function, direction, start, end, duration);
                tween.Initialize(GetComponentValue(component, unit));

                if (tweens.ContainsKey((component, unit))) tweens[(component, unit)] = tween;
                else tweens.Add((component, unit), tween);
            }
            public void Finish(TransitionComponents component, TransitionUnits unit)
            {
                if (!tweens.TryGetValue((component, unit), out Tween tween)) { Debug.LogError($"Failed transitioning object. FEC: Object tween not found : {component}.{unit} in {gameObject.name}"); return; }

                tweens.Remove((component, unit));
                SetComponentValue(component, unit, tween.end);
            }

            public int IncrementTweens(float? time = null)
            {
                if (time == null) time = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                List<(TransitionComponents component, TransitionUnits unit)> finished = new List<(TransitionComponents, TransitionUnits)>();

                foreach (var kvp in tweens)
                {
                    TransitionComponents component = kvp.Key.component;
                    TransitionUnits unit = kvp.Key.unit;
                    Tween tween = kvp.Value;

                    SetComponentValue(component, unit, tween.Increment(time.Value));
                    if (tween.duration == tween.timer) finished.Add(kvp.Key);
                }

                foreach (var key in finished)
                    Finish(key.component, key.unit);

                return tweens.Count;
            }

            public void Stop()
            {
                if (objects.ContainsKey(gameObject))
                    objects.Remove(gameObject);
            }


            public float GetComponentValue(TransitionComponents component, TransitionUnits unit)
            {
                switch (component)
                {
                    case TransitionComponents.Position:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. GCV: Object does not contain {component} component"); return 0; }
                        else switch (unit)
                            {
                                case TransitionUnits.X: return transform.position.x;
                                case TransitionUnits.Y: return transform.position.y;
                                case TransitionUnits.Z: return transform.position.z;
                                default: Debug.LogError($"Failed transitioning object. GCV: Unit {unit} not found in Component {component}"); return 0;
                            }

                    case TransitionComponents.LocalPosition:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. GCV: Object does not contain {component} component"); return 0; }
                        else switch (unit)
                            {
                                case TransitionUnits.X: return transform.localPosition.x;
                                case TransitionUnits.Y: return transform.localPosition.y;
                                case TransitionUnits.Z: return transform.localPosition.z;
                                default: Debug.LogError($"Failed transitioning object. GCV: Unit {unit} not found in Component {component}"); return 0;
                            }

                    case TransitionComponents.Rotation:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. GCV: Object does not contain {component} component"); return 0; }
                        else switch (unit)
                            {
                                case TransitionUnits.X: return transform.localEulerAngles.x;
                                case TransitionUnits.Y: return transform.localEulerAngles.y;
                                case TransitionUnits.Z: return transform.localEulerAngles.z;
                                default: Debug.LogError($"Failed transitioning object. GCV: Unit {unit} not found in Component {component}"); return 0;
                            }

                    case TransitionComponents.Scale:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. GCV: Object does not contain {component} component"); return 0; }
                        else switch (unit)
                            {
                                case TransitionUnits.X: return transform.localScale.x;
                                case TransitionUnits.Y: return transform.localScale.y;
                                case TransitionUnits.Z: return transform.localScale.z;
                                default: Debug.LogError($"Failed transitioning object. GCV: Unit {unit} not found in Component {component}"); return 0;
                            }

                    case TransitionComponents.UIColor:
                        if (image == null) { Debug.LogError($"Failed transitioning object. GCV: Object does not contain {component} component"); return 0; }
                        else switch (unit)
                            {
                                case TransitionUnits.R: return image.color.r;
                                case TransitionUnits.G: return image.color.g;
                                case TransitionUnits.B: return image.color.b;
                                case TransitionUnits.A: return image.color.a;
                                default: Debug.LogError($"Failed transitioning object. GCV: Unit {unit} not found in Component {component}"); return 0;
                            }

                    case TransitionComponents.UIAlpha:
                        if (canvasGroup == null) { Debug.LogError($"Failed transitioning object. GCV: Object does not contain {component} component"); return 0; }
                        else return canvasGroup.alpha;

                    default: Debug.LogError($"Failed transitioning object. GCV: Component {component} not supported"); return 0;
                }
            }
            public void SetComponentValue(TransitionComponents component, TransitionUnits unit, float value)
            {
                switch (component)
                {
                    case TransitionComponents.Position:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. SCV: Object does not contain {component} component"); }
                        else switch (unit)
                            {
                                case TransitionUnits.X: SetPosition(value, null, null); break;
                                case TransitionUnits.Y: SetPosition(null, value, null); break;
                                case TransitionUnits.Z: SetPosition(null, null, value); break;
                                default: Debug.LogError($"Failed transitioning object. SCV: Unit {unit} not found in Component {component}"); break;
                            }
                        break;

                    case TransitionComponents.LocalPosition:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. SCV: Object does not contain {component} component"); }
                        else switch (unit)
                            {
                                case TransitionUnits.X: SetLocalPosition(value, null, null); break;
                                case TransitionUnits.Y: SetLocalPosition(null, value, null); break;
                                case TransitionUnits.Z: SetLocalPosition(null, null, value); break;
                                default: Debug.LogError($"Failed transitioning object. SCV: Unit {unit} not found in Component {component}"); break;
                            }
                        break;

                    case TransitionComponents.Rotation:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. SCV: Object does not contain {component} component"); }
                        else switch (unit)
                            {
                                case TransitionUnits.X: SetRotation(value, null, null); break;
                                case TransitionUnits.Y: SetRotation(null, value, null); break;
                                case TransitionUnits.Z: SetRotation(null, null, value); break;
                                default: Debug.LogError($"Failed transitioning object. SCV: Unit {unit} not found in Component {component}"); break;
                            }
                        break;

                    case TransitionComponents.Scale:
                        if (transform == null) { Debug.LogError($"Failed transitioning object. SCV: Object does not contain {component} component"); }
                        else switch (unit)
                            {
                                case TransitionUnits.X: SetScale(value, null, null); break;
                                case TransitionUnits.Y: SetScale(null, value, null); break;
                                case TransitionUnits.Z: SetScale(null, null, value); break;
                                default: Debug.LogError($"Failed transitioning object. SCV: Unit {unit} not found in Component {component}"); break;
                            }
                        break;

                    case TransitionComponents.UIColor:
                        if (image == null) { Debug.LogError($"Failed transitioning object. SCV: Object does not contain {component} component"); }
                        else switch (unit)
                            {
                                case TransitionUnits.R: SetUIColor(value, null, null, null); break;
                                case TransitionUnits.G: SetUIColor(null, value, null, null); break;
                                case TransitionUnits.B: SetUIColor(null, null, value, null); break;
                                case TransitionUnits.A: SetUIColor(null, null, null, value); break;
                                default: Debug.LogError($"Failed transitioning object. SCV: Unit {unit} not found in Component {component}"); break;
                            }
                        break;

                    case TransitionComponents.UIAlpha:
                        if (canvasGroup == null) { Debug.LogError($"Failed transitioning object. SCV: Object does not contain {component} component"); }
                        else canvasGroup.alpha = value;
                        break;

                    default: Debug.LogError($"Failed transitioning object. SCV: Component {component} not supported"); break;
                }
            }


            public void SetPosition(float? x = null, float? y = null, float? z = null)
            {
                if (x == null) x = transform.position.x;
                if (y == null) y = transform.position.y;
                if (z == null) z = transform.position.z;

                SetPosition(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetPosition(Vector3 position) => transform.position = position;

            public void SetLocalPosition(float? x = null, float? y = null, float? z = null)
            {
                if (x == null) x = transform.localPosition.x;
                if (y == null) y = transform.localPosition.y;
                if (z == null) z = transform.localPosition.z;

                SetLocalPosition(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetLocalPosition(Vector3 position) => transform.localPosition = position;

            public void SetRotation(float? x = null, float? y = null, float? z = null)
            {
                if (x == null) x = transform.localEulerAngles.x;
                if (y == null) y = transform.localEulerAngles.y;
                if (z == null) z = transform.localEulerAngles.z;

                SetRotation(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetRotation(Vector3 rotation) => transform.localEulerAngles = rotation;

            public void SetScale(float? x = null, float? y = null, float? z = null)
            {
                if (x == null) x = transform.localScale.x;
                if (y == null) y = transform.localScale.y;
                if (z == null) z = transform.localScale.z;

                SetScale(new Vector3(x.Value, y.Value, z.Value));
            }
            public void SetScale(float scale) => SetScale(new Vector3(scale, scale, scale));
            public void SetScale(Vector3 scale) => transform.localScale = scale;

            public void SetUIColor(float? r = null, float? g = null, float? b = null, float? a = null)
            {
                if (r == null) r = image.color.r;
                if (g == null) g = image.color.g;
                if (b == null) b = image.color.b;
                if (a == null) a = image.color.a;

                SetUIColor(new Color(r.Value, g.Value, b.Value, a.Value));
            }
            public void SetUIColor(Color color) => image.color = color;
        }
        #endregion Object
        #region Tween
        public class Tween
        {
            public EaseFunctions function;
            public EaseDirections direction;
            public float start;
            public float end;
            public float duration;
            public float timer;

            public Action<float> action;


            public Tween(EaseFunctions function, EaseDirections direction, float start, float end, float duration)
            {
                this.function = function;
                this.direction = direction;
                this.start = start;
                this.end = end;
                this.duration = duration;
                this.timer = 0;
            }

            public void Initialize(float value)
            {
                #region Clamp Start
                if (start < end)
                {
                    if (value < end)
                    {
                        if (value > start)
                        { }
                        else
                            start = value;
                    }
                    else if (value > end)
                    {
                        if (value < end + (end - start))
                            start = end + (end - start);
                        else
                            start = value;
                    }
                }
                else
                {
                    if (value > end)
                    {
                        if (value < start)
                        { }
                        else
                            start = value;
                    }
                    else if (value < end)
                    {
                        if (value > end - (start - end))
                            start = end - (start - end);
                        else
                            start = value;
                    }
                }
                #endregion Clamp Start

                float y = Mathf.InverseLerp(start, end, value);
                float x = CalculateEaseInverse(function, direction, y);

                timer = Mathf.Lerp(0, duration, x);
            }
            public float Increment(float time)
            {
                timer = Mathf.Clamp(timer += time, 0, duration);
                float x = Mathf.InverseLerp(0, duration, timer);
                float y = CalculateEase(function, direction, x);

                float value = Mathf.Lerp(start, end, y);
                action?.Invoke(value);
                return value;
            }

            public void Stop()
            {
                if (tweens.Contains(this))
                    tweens.Remove(this);
            }


            #region Ease Functions
            float CalculateEase(EaseFunctions function, EaseDirections direction, float x)
            {
                switch (function)
                {
                    case EaseFunctions.Linear:
                        return Linear(x);
                    case EaseFunctions.Quadratic:
                        switch (direction) { case EaseDirections.In: return QuadraticIn(x); case EaseDirections.Out: return QuadraticOut(x); case EaseDirections.InOut: return QuadraticInOut(x); default: return 0; };
                    case EaseFunctions.Cubic:
                        switch (direction) { case EaseDirections.In: return CubicIn(x); case EaseDirections.Out: return CubicOut(x); case EaseDirections.InOut: return CubicInOut(x); default: return 0; };
                    case EaseFunctions.Quartic:
                        switch (direction) { case EaseDirections.In: return QuarticIn(x); case EaseDirections.Out: return QuarticOut(x); case EaseDirections.InOut: return QuarticInOut(x); default: return 0; };
                    case EaseFunctions.Quintic:
                        switch (direction) { case EaseDirections.In: return QuinticIn(x); case EaseDirections.Out: return QuinticOut(x); case EaseDirections.InOut: return QuinticInOut(x); default: return 0; };
                    case EaseFunctions.Sine:
                        switch (direction) { case EaseDirections.In: return SineIn(x); case EaseDirections.Out: return SineOut(x); case EaseDirections.InOut: return SineInOut(x); default: return 0; };
                    case EaseFunctions.Circular:
                        switch (direction) { case EaseDirections.In: return CircularIn(x); case EaseDirections.Out: return CircularOut(x); case EaseDirections.InOut: return CircularInOut(x); default: return 0; };
                    case EaseFunctions.Exponential:
                        switch (direction) { case EaseDirections.In: return ExponentialIn(x); case EaseDirections.Out: return ExponentialOut(x); case EaseDirections.InOut: return ExponentialInOut(x); default: return 0; };
                    case EaseFunctions.Elastic:
                        switch (direction) { case EaseDirections.In: return ElasticIn(x); case EaseDirections.Out: return ElasticOut(x); case EaseDirections.InOut: return ElasticInOut(x); default: return 0; };
                    case EaseFunctions.Back:
                        switch (direction) { case EaseDirections.In: return BackIn(x); case EaseDirections.Out: return BackOut(x); case EaseDirections.InOut: return BackInOut(x); default: return 0; };
                    case EaseFunctions.Bounce:
                        switch (direction) { case EaseDirections.In: return BounceIn(x); case EaseDirections.Out: return BounceOut(x); case EaseDirections.InOut: return BounceInOut(x); default: return 0; };
                    default: return 0;
                }
            }
            float CalculateEaseInverse(EaseFunctions function, EaseDirections direction, float x)
            {
                switch (function)
                {
                    case EaseFunctions.Linear:
                        return Linear(x);
                    case EaseFunctions.Quadratic:
                        switch (direction) { case EaseDirections.In: return QuadraticInInverse(x); case EaseDirections.Out: return QuadraticOutInverse(x); case EaseDirections.InOut: return QuadraticInOutInverse(x); default: return 0; };
                    case EaseFunctions.Cubic:
                        switch (direction) { case EaseDirections.In: return CubicInInverse(x); case EaseDirections.Out: return CubicOutInverse(x); case EaseDirections.InOut: return CubicInOutInverse(x); default: return 0; };
                    case EaseFunctions.Quartic:
                        switch (direction) { case EaseDirections.In: return QuarticInInverse(x); case EaseDirections.Out: return QuarticOutInverse(x); case EaseDirections.InOut: return QuarticInOutInverse(x); default: return 0; };
                    case EaseFunctions.Quintic:
                        switch (direction) { case EaseDirections.In: return QuinticInInverse(x); case EaseDirections.Out: return QuinticOutInverse(x); case EaseDirections.InOut: return QuinticInOutInverse(x); default: return 0; };
                    case EaseFunctions.Sine:
                        switch (direction) { case EaseDirections.In: return SineInInverse(x); case EaseDirections.Out: return SineOutInverse(x); case EaseDirections.InOut: return SineInOutInverse(x); default: return 0; };
                    case EaseFunctions.Circular:
                        switch (direction) { case EaseDirections.In: return CircularInInverse(x); case EaseDirections.Out: return CircularOutInverse(x); case EaseDirections.InOut: return CircularInOutInverse(x); default: return 0; };
                    case EaseFunctions.Exponential:
                        switch (direction) { case EaseDirections.In: return ExponentialInInverse(x); case EaseDirections.Out: return ExponentialOutInverse(x); case EaseDirections.InOut: return ExponentialInOutInverse(x); default: return 0; };
                    case EaseFunctions.Elastic:
                        switch (direction) { case EaseDirections.In: return ElasticInInverse(x); case EaseDirections.Out: return ElasticOutInverse(x); case EaseDirections.InOut: return ElasticInOutInverse(x); default: return 0; };
                    case EaseFunctions.Back:
                        switch (direction) { case EaseDirections.In: return BackInInverse(x); case EaseDirections.Out: return BackOutInverse(x); case EaseDirections.InOut: return BackInOutInverse(x); default: return 0; };
                    case EaseFunctions.Bounce:
                        switch (direction) { case EaseDirections.In: return BounceInInverse(x); case EaseDirections.Out: return BounceOutInverse(x); case EaseDirections.InOut: return BounceInOutInverse(x); default: return 0; };
                    default: return 0;
                }
            }

            #region Misc
            float Linear(float x)
            {
                return x;
            }
            #endregion Misc

            #region Quadratic
            float QuadraticIn(float x)
            {
                return x * x;
            }
            float QuadraticOut(float x)
            {
                return x * (2 - x);
            }
            float QuadraticInOut(float x)
            {
                if (x < 0.5f)
                    return 2 * x * x;

                return (-2 * x * x) + (4 * x) - 1;
            }

            float QuadraticInInverse(float x)
            {
                return Mathf.Sqrt(x);
            }
            float QuadraticOutInverse(float x)
            {
                return 1 - Mathf.Sqrt(-x + 1);
            }
            float QuadraticInOutInverse(float x)
            {
                if (x < 0.5f)
                    return Mathf.Sqrt(x / 2);

                return (-4 + Mathf.Sqrt(8 * (-x + 1))) / -4;
            }
            #endregion Quadratic

            #region Cubic
            float CubicIn(float x)
            {
                return x * x * x;
            }
            float CubicOut(float x)
            {
                x = x - 1;
                return x * x * x + 1;
            }
            float CubicInOut(float x)
            {
                if (x < 0.5f)
                    return 4 * x * x * x;

                x = 2 * x - 2;
                return 0.5f * x * x * x + 1;
            }

            float CubicInInverse(float x)
            {
                return Mathf.Pow(x, 1f / 3f);
            }
            float CubicOutInverse(float x)
            {
                x = x - 1;
                return -Mathf.Pow(-x, 1f / 3f) + 1;
            }
            float CubicInOutInverse(float x)
            {
                if (x < 0.5f)
                    return Mathf.Pow(x / 4f, 1f / 3f);

                x = x - 1;
                return 0.62996f * -Mathf.Pow(-x, 1f / 3f) + 1;
            }
            #endregion Cubic

            #region Quartic
            float QuarticIn(float x)
            {
                return x * x * x * x;
            }
            float QuarticOut(float x)
            {
                x = x - 1;
                return 1 - (x * x * x * x);
            }
            float QuarticInOut(float x)
            {
                if (x < 0.5f)
                    return 8 * x * x * x * x;

                x = x - 1;
                return -8 * x * x * x * x + 1;
            }

            float QuarticInInverse(float x)
            {
                return Mathf.Pow(x, 1f / 4f);
            }
            float QuarticOutInverse(float x)
            {
                x = -x + 1;
                return -Mathf.Pow(x, 1f / 4f) + 1;
            }
            float QuarticInOutInverse(float x)
            {
                if (x < 0.5f)
                    return Mathf.Pow(x / 8f, 1f / 4f);

                x = -(x - 1) / 8f;
                return -Mathf.Pow(x, 1f / 4f) + 1;
            }
            #endregion Quartic

            #region Quintic
            float QuinticIn(float x)
            {
                return x * x * x * x * x;
            }
            float QuinticOut(float x)
            {
                x = x - 1;
                return x * x * x * x * x + 1;
            }
            float QuinticInOut(float x)
            {
                if (x < 0.5f)
                    return 16 * x * x * x * x * x;

                x = 2 * x - 2;
                return 0.5f * x * x * x * x * x + 1;
            }

            float QuinticInInverse(float x)
            {
                return Mathf.Pow(x, 1f / 5f);
            }
            float QuinticOutInverse(float x)
            {
                x = x - 1;
                return -Mathf.Pow(-x, 1f / 5f) + 1;
            }
            float QuinticInOutInverse(float x)
            {
                if (x < 0.5f)
                    return Mathf.Pow(x / 16f, 1f / 5f);

                x = x - 1;
                return 0.57435f * -Mathf.Pow(-x, 1f / 5f) + 1;
            }
            #endregion Quintic

            #region Sine
            float SineIn(float x)
            {
                return Mathf.Sin((x - 1) * (Mathf.PI / 2)) + 1;
            }
            float SineOut(float x)
            {
                return Mathf.Sin(x * (Mathf.PI / 2));
            }
            float SineInOut(float x)
            {
                return 0.5f * (1 - Mathf.Cos(x * Mathf.PI));
            }

            float SineInInverse(float x)
            {
                return (2 / Mathf.PI) * Mathf.Asin(x - 1) + 1;
            }
            float SineOutInverse(float x)
            {
                return (2 / Mathf.PI) * Mathf.Asin(x);
            }
            float SineInOutInverse(float x)
            {
                return (1 / Mathf.PI) * Mathf.Acos(1 - (2 * x));
            }
            #endregion Sine

            #region Circular
            float CircularIn(float x)
            {
                return 1 - Mathf.Sqrt(1 - (x * x));
            }
            float CircularOut(float x)
            {
                return Mathf.Sqrt((2 - x) * x);
            }
            float CircularInOut(float x)
            {
                if (x < 0.5f)
                    return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (x * x)));

                return 0.5f * (Mathf.Sqrt(-((2 * x) - 3) * ((2 * x) - 1)) + 1);
            }

            float CircularInInverse(float x)
            {
                return Mathf.Sqrt(x * (2 - x));
            }
            float CircularOutInverse(float x)
            {
                return 1 - Mathf.Sqrt(1 - (x * x));
            }
            float CircularInOutInverse(float x)
            {
                if (x < 0.5f)
                    return Mathf.Sqrt(-x * (x - 1));

                return 1 - Mathf.Sqrt(x * (-x + 1));
            }
            #endregion Circular

            // Inverse Functions WIP

            #region Exponential
            float ExponentialIn(float x)
            {
                if (x == 0)
                    return x;

                return Mathf.Pow(2, 10 * (x - 1));
            }
            float ExponentialOut(float x)
            {
                if (x == 1)
                    return x;

                return 1 - Mathf.Pow(2, -10 * x);
            }
            float ExponentialInOut(float x)
            {
                if (x == 0 || x == 1)
                    return x;

                if (x < 0.5f)
                    return 0.5f * Mathf.Pow(2, (20 * x) - 10);

                return -0.5f * Mathf.Pow(2, (-20 * x) + 10) + 1;
            }

            float ExponentialInInverse(float x)
            {
                return x;
            }
            float ExponentialOutInverse(float x)
            {
                return x;
            }
            float ExponentialInOutInverse(float x)
            {
                return x;
            }
            #endregion Exponential

            #region Elastic
            float ElasticIn(float x)
            {
                return Mathf.Sin(13 * (Mathf.PI / 2) * x) * Mathf.Pow(2, 10 * (x - 1));
            }
            float ElasticOut(float x)
            {
                return Mathf.Sin(-13 * (Mathf.PI / 2) * (x + 1)) * Mathf.Pow(2, -10 * x) + 1;
            }
            float ElasticInOut(float x)
            {
                if (x < 0.5f)
                    return 0.5f * Mathf.Sin(13 * (Mathf.PI / 2) * (2 * x)) * Mathf.Pow(2, 10 * ((2 * x) - 1));

                return 0.5f * (Mathf.Sin(-13 * (Mathf.PI / 2) * ((2 * x - 1) + 1)) * Mathf.Pow(2, -10 * (2 * x - 1)) + 2);
            }

            float ElasticInInverse(float x)
            {
                return x;
            }
            float ElasticOutInverse(float x)
            {
                return x;
            }
            float ElasticInOutInverse(float x)
            {
                return x;
            }
            #endregion Elastic

            #region Back
            float BackIn(float x)
            {
                return x * x * x - x * Mathf.Sin(x * Mathf.PI);
            }
            float BackOut(float x)
            {
                x = -x + 1;
                return 1 - (x * x * x - x * Mathf.Sin(x * Mathf.PI));
            }
            float BackInOut(float x)
            {
                x = x * 2;
                if (x < 1)
                    return 0.5f * (x * x * x - x * Mathf.Sin(x * Mathf.PI));

                x = -(x - 2);
                return 0.5f * (1 - (x * x * x - x * Mathf.Sin(x * Mathf.PI))) + 0.5f;
            }

            float BackInInverse(float x)
            {
                return x;
            }
            float BackOutInverse(float x)
            {
                return x;
            }
            float BackInOutInverse(float x)
            {
                return x;
            }
            #endregion Back

            #region Bounce
            float BounceIn(float x)
            {
                return 1 - BounceOut(1 - x);
            }
            float BounceOut(float x)
            {
                if (x < 0.3636f)
                    return (121 * x * x) / 16.0f;
                else if (x < 0.7272f)
                    return (9.075f * x * x) - (9.9f * x) + 3.4f;
                else if (x < 0.9f)
                    return (12.0665f * x * x) - (19.6355f * x) + 8.8981f;
                else
                    return (10.8f * x * x) - (20.52f * x) + 10.72f;
            }
            float BounceInOut(float x)
            {
                if (x < 0.5f)
                    return 0.5f * BounceIn(x * 2);

                return 0.5f * BounceOut(x * 2 - 1) + 0.5f;
            }

            float BounceInInverse(float x)
            {
                return x;
            }
            float BounceOutInverse(float x)
            {
                return x;
            }
            float BounceInOutInverse(float x)
            {
                return x;
            }
            #endregion Bounce
            #endregion Ease Functions
        }
        #endregion Tween

        public static Dictionary<GameObject, Object> objects;
        public static List<Tween> tweens;

        public static void Initialize()
        {
            objects = new Dictionary<GameObject, Object>();
            tweens = new List<Tween>();
        }
        public static void IncrementObjects()
        {
            if (objects.Count == 0) return;
            Dictionary<GameObject, Object> newObjects = new Dictionary<GameObject, Object>();

            foreach (KeyValuePair<GameObject, Object> kvp in objects)
                if (kvp.Key != null && kvp.Value.IncrementTweens() != 0)
                    newObjects.Add(kvp.Key, kvp.Value);

            objects = newObjects;
        }
        public static void IncrementTweens()
        {
            if (tweens.Count == 0) return;
            List<Tween> newTweens = new List<Tween>();

            foreach (Tween tween in tweens)
            {
                tween.Increment(Time.deltaTime);

                if (tween.duration != tween.timer)
                    newTweens.Add(tween);
            }

            tweens = newTweens;
        }

        public static Object Add(GameObject gameObject, TransitionComponents component, TransitionUnits unit, EaseFunctions function, EaseDirections direction, float start, float end, float duration, bool? ignoreTimeScale = null)
        {
            if (!objects.TryGetValue(gameObject, out Object obj))
            {
                obj = new Object(gameObject);
                objects.Add(gameObject, obj);
            }

            obj.Set(component, unit, function, direction, start, end, duration);
            if (ignoreTimeScale != null) obj.ignoreTimeScale = ignoreTimeScale.Value;
            return obj;
        }
        public static void Remove(GameObject gameObject)
        {
            if (objects.ContainsKey(gameObject))
                objects.Remove(gameObject);
        }

        public static Tween Add(Action<float> action, EaseFunctions function, EaseDirections direction, float start, float end, float duration)
        {
            Tween tween = new Tween(function, direction, start, end, duration);
            tween.action = action;

            tween.Initialize(start);
            tweens.Add(tween);
            return tween;
        }
    }
    #endregion Transition
}