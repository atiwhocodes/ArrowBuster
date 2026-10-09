using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ArrowBuster
{
    /// <summary>
    /// Gameplay HUD and result screens (05 §14–15, mvp.md §8), built in code from <see cref="UiFactory"/>:
    /// top bar (pause, level, restart), objective and quiver chips, level banner, toasts, tutorial callout and ghost
    /// hand, and the Win / Fail / Pause / slice-complete panels. Listens to <see cref="GameEvents"/>; issues
    /// commands to <see cref="GameplayController"/>.
    /// </summary>
    public sealed class GameplayUI : MonoBehaviour
    {
        private const int MaxQuiverIcons = 8;

        [SerializeField] private GameplayController _controller;
        [SerializeField] private TutorialPromptController _tutorial;

        private RectTransform _safe;
        private TextMeshProUGUI _levelLabel;
        private RectTransform _objectiveRow;
        private readonly List<Image> _objectiveIcons = new List<Image>();
        private readonly List<Image> _objectiveMarks = new List<Image>();
        private Image _protectedIcon;
        private Image _protectedMark;
        private readonly List<Image> _quiverIcons = new List<Image>();
        private TextMeshProUGUI _quiverCount;

        private CanvasGroup _banner;
        private TextMeshProUGUI _bannerText;
        private CanvasGroup _toast;
        private TextMeshProUGUI _toastText;
        private CanvasGroup _callout;
        private TextMeshProUGUI _calloutText;
        private Transform _calloutAnchor;
        private CanvasGroup _ghost;
        private RectTransform _ghostHand;

        private CanvasGroup _winPanel;
        private TextMeshProUGUI _winTitle;
        private TextMeshProUGUI _winDetail;
        private readonly Image[] _winStars = new Image[3];
        private Button _nextButton;
        private TextMeshProUGUI _nextLabel;

        private CanvasGroup _failPanel;
        private TextMeshProUGUI _failTitle;
        private TextMeshProUGUI _failDetail;

        private CanvasGroup _pausePanel;
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _musicLabel;
        private TextMeshProUGUI _hapticsLabel;

        private CanvasGroup _slicePanel;
        private RectTransform _sliceList;

        private void Awake()
        {
            EnsureEventSystem();
            BuildCanvas();
        }

        private void OnEnable()
        {
            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.ArrowFired += OnArrowFired;
            GameEvents.ObjectiveCleared += OnObjectiveCleared;
            GameEvents.ProtectedLost += OnProtectedLost;
            GameEvents.SoftLockPrompt += OnSoftLock;
            GameEvents.GameplayStateChanged += OnStateChanged;
            GameEvents.LevelWon += OnLevelWon;
            GameEvents.LevelFailed += OnLevelFailed;
            if (_tutorial != null)
            {
                _tutorial.GhostHandChanged += OnGhostHand;
                _tutorial.CalloutChanged += OnCallout;
            }
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.ArrowFired -= OnArrowFired;
            GameEvents.ObjectiveCleared -= OnObjectiveCleared;
            GameEvents.ProtectedLost -= OnProtectedLost;
            GameEvents.SoftLockPrompt -= OnSoftLock;
            GameEvents.GameplayStateChanged -= OnStateChanged;
            GameEvents.LevelWon -= OnLevelWon;
            GameEvents.LevelFailed -= OnLevelFailed;
            if (_tutorial != null)
            {
                _tutorial.GhostHandChanged -= OnGhostHand;
                _tutorial.CalloutChanged -= OnCallout;
            }
        }

        // ------------------------------------------------------------------ build

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("GameplayCanvas", typeof(RectTransform));
            canvasGo.layer = 5;
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _safe = UiFactory.Stretch(UiFactory.Rect(canvasGo.transform, "SafeArea"));
            _safe.gameObject.AddComponent<SafeAreaFitter>();

            BuildHud();
            BuildOverlays();
            BuildWinPanel();
            BuildFailPanel();
            BuildPausePanel();
            BuildSlicePanel();
        }

        private void BuildHud()
        {
            RectTransform top = UiFactory.Rect(_safe, "TopBar");
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 300f);
            top.anchoredPosition = Vector2.zero;

            Button pause = UiFactory.IconButton(top, "Pause", ProceduralTextures.PauseIcon, UiTheme.HudChip, Color.white, 118f, () => _controller.Pause());
            UiFactory.Place((RectTransform)pause.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -28f), new Vector2(118f, 118f));

            Button restart = UiFactory.IconButton(top, "Restart", ProceduralTextures.RestartIcon, UiTheme.HudChip, Color.white, 118f, () => _controller.Retry());
            UiFactory.Place((RectTransform)restart.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -28f), new Vector2(118f, 118f));

            Image levelChip = UiFactory.Image(top, "LevelChip", ProceduralTextures.Pill, UiTheme.HudChip);
            UiFactory.Place(levelChip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(600f, 96f));
            _levelLabel = UiFactory.Label(levelChip.transform, "Level", "Level 1", 42f, Color.white);
            UiFactory.Stretch(_levelLabel.rectTransform, 12f);

            Image objectivesChip = UiFactory.Image(top, "ObjectivesChip", ProceduralTextures.Pill, UiTheme.HudChip);
            UiFactory.Place(objectivesChip.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -172f), new Vector2(420f, 104f));
            _objectiveRow = UiFactory.Stretch(UiFactory.Rect(objectivesChip.transform, "Row"), 14f);
            var layout = _objectiveRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(16, 16, 0, 0);

            Image quiverChip = UiFactory.Image(top, "QuiverChip", ProceduralTextures.Pill, UiTheme.HudChip);
            UiFactory.Place(quiverChip.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -172f), new Vector2(420f, 104f));
            RectTransform quiverRow = UiFactory.Stretch(UiFactory.Rect(quiverChip.transform, "Row"), 14f);
            var quiverLayout = quiverRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            quiverLayout.spacing = 2f;
            quiverLayout.childAlignment = TextAnchor.MiddleRight;
            quiverLayout.childControlWidth = false;
            quiverLayout.childControlHeight = false;
            quiverLayout.childForceExpandWidth = false;
            quiverLayout.padding = new RectOffset(10, 18, 0, 0);
            _quiverCount = UiFactory.Label(quiverRow, "Count", "×3", 46f, Color.white);
            _quiverCount.rectTransform.sizeDelta = new Vector2(96f, 80f);
            for (int i = 0; i < MaxQuiverIcons; i++)
            {
                Image icon = UiFactory.Image(quiverRow, "Arrow" + i, ProceduralTextures.ArrowIcon, Color.white);
                icon.rectTransform.sizeDelta = new Vector2(38f, 80f);
                icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -18f);
                _quiverIcons.Add(icon);
            }
        }

        private void BuildOverlays()
        {
            Image banner = UiFactory.Image(_safe, "Banner", ProceduralTextures.Pill, new Color(0.18f, 0.12f, 0.07f, 0.85f));
            UiFactory.Place(banner.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 150f));
            _bannerText = UiFactory.Label(banner.transform, "Text", "", 60f, Color.white);
            UiFactory.Stretch(_bannerText.rectTransform, 16f);
            _banner = UiFactory.Group(banner.gameObject);
            _banner.blocksRaycasts = false;
            UiFactory.SetVisible(_banner, false);

            Image toast = UiFactory.Image(_safe, "Toast", ProceduralTextures.Pill, new Color(0.18f, 0.12f, 0.07f, 0.85f));
            UiFactory.Place(toast.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 120f));
            _toastText = UiFactory.Label(toast.transform, "Text", "", 50f, UiTheme.Gold);
            UiFactory.Stretch(_toastText.rectTransform, 12f);
            _toast = UiFactory.Group(toast.gameObject);
            _toast.blocksRaycasts = false;
            UiFactory.SetVisible(_toast, false);

            Image callout = UiFactory.Image(_safe, "Callout", ProceduralTextures.Pill, UiTheme.Parchment);
            UiFactory.Place(callout.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(560f, 116f));
            Image pointer = UiFactory.Image(callout.transform, "Pointer", ProceduralTextures.Circle, UiTheme.Parchment);
            UiFactory.Place(pointer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(34f, 34f));
            _calloutText = UiFactory.Label(callout.transform, "Text", "", 46f, UiTheme.Ink);
            UiFactory.Stretch(_calloutText.rectTransform, 12f);
            _callout = UiFactory.Group(callout.gameObject);
            _callout.blocksRaycasts = false;
            UiFactory.SetVisible(_callout, false);

            RectTransform ghostRoot = UiFactory.Rect(_safe, "GhostHand");
            UiFactory.Place(ghostRoot, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 10f));
            Image hand = UiFactory.Image(ghostRoot, "Hand", ProceduralTextures.Hand, new Color(1f, 1f, 1f, 0.92f));
            hand.rectTransform.sizeDelta = new Vector2(190f, 190f);
            _ghostHand = hand.rectTransform;
            _ghost = UiFactory.Group(ghostRoot.gameObject);
            _ghost.blocksRaycasts = false;
            UiFactory.SetVisible(_ghost, false);
        }

        private RectTransform BuildCard(string name, out CanvasGroup group, float height)
        {
            Image shade = UiFactory.Image(_safe.parent, name, null, UiTheme.Shade, raycast: true);
            UiFactory.Stretch(shade.rectTransform);
            group = UiFactory.Group(shade.gameObject);
            Image card = UiFactory.Image(shade.transform, "Card", ProceduralTextures.Panel, UiTheme.Parchment, raycast: true);
            UiFactory.Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(880f, height));
            Image rim = UiFactory.Image(card.transform, "Rim", ProceduralTextures.Panel, UiTheme.ParchmentDark);
            UiFactory.Stretch(rim.rectTransform, -10f);
            rim.transform.SetAsFirstSibling();
            UiFactory.SetVisible(group, false);
            return card.rectTransform;
        }

        private void BuildWinPanel()
        {
            RectTransform card = BuildCard("WinPanel", out _winPanel, 980f);
            _winTitle = UiFactory.Label(card, "Title", "Level Clear!", 86f, UiTheme.Ink);
            UiFactory.Place(_winTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 120f));
            for (int i = 0; i < 3; i++)
            {
                Image star = UiFactory.Image(card, "Star" + i, ProceduralTextures.Star, UiTheme.StarEmpty);
                float x = (i - 1) * 230f;
                float y = i == 1 ? -230f : -270f;
                float size = i == 1 ? 230f : 190f;
                UiFactory.Place(star.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(x, y - 60f), new Vector2(size, size));
                _winStars[i] = star;
            }
            _winDetail = UiFactory.Label(card, "Detail", "", 46f, UiTheme.InkSoft, style: FontStyles.Normal);
            UiFactory.Place(_winDetail.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(800f, 140f));

            _nextButton = UiFactory.Button(card, "Next", "Next", UiTheme.Gold, UiTheme.Ink, new Vector2(600f, 160f), OnNextClicked, 70f);
            UiFactory.Place((RectTransform)_nextButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 220f), new Vector2(600f, 160f));
            _nextLabel = _nextButton.GetComponentInChildren<TextMeshProUGUI>();
            Button replay = UiFactory.Button(card, "Replay", "Replay", UiTheme.ParchmentDark, UiTheme.Ink, new Vector2(420f, 120f), () => _controller.Retry(), 50f);
            UiFactory.Place((RectTransform)replay.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(420f, 120f));
        }

        private void BuildFailPanel()
        {
            RectTransform card = BuildCard("FailPanel", out _failPanel, 760f);
            _failTitle = UiFactory.Label(card, "Title", "Out of arrows", 80f, UiTheme.Ink);
            UiFactory.Place(_failTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(820f, 120f));
            _failDetail = UiFactory.Label(card, "Detail", "", 46f, UiTheme.InkSoft, style: FontStyles.Normal);
            UiFactory.Place(_failDetail.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(780f, 200f));
            Button retry = UiFactory.Button(card, "Retry", "Retry", UiTheme.Gold, UiTheme.Ink, new Vector2(620f, 170f), () => _controller.Retry(), 74f);
            UiFactory.Place((RectTransform)retry.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(620f, 170f));
        }

        private void BuildPausePanel()
        {
            RectTransform card = BuildCard("PausePanel", out _pausePanel, 1060f);
            TextMeshProUGUI title = UiFactory.Label(card, "Title", "Paused", 86f, UiTheme.Ink);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 120f));
            Button resume = UiFactory.Button(card, "Resume", "Resume", UiTheme.Gold, UiTheme.Ink, new Vector2(600f, 150f), () => _controller.Resume(), 64f);
            UiFactory.Place((RectTransform)resume.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(600f, 150f));
            Button restart = UiFactory.Button(card, "Restart", "Restart level", UiTheme.ParchmentDark, UiTheme.Ink, new Vector2(600f, 120f), () =>
            {
                _controller.Resume();
                _controller.Retry();
            }, 50f);
            UiFactory.Place((RectTransform)restart.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -390f), new Vector2(600f, 120f));

            _soundLabel = Toggle(card, "Sound", -560f, ToggleSound);
            _musicLabel = Toggle(card, "Music", -700f, ToggleMusic);
            _hapticsLabel = Toggle(card, "Haptics", -840f, ToggleHaptics);
        }

        private TextMeshProUGUI Toggle(RectTransform card, string name, float y, UnityEngine.Events.UnityAction onClick)
        {
            Button button = UiFactory.Button(card, name, name, UiTheme.ParchmentDark, UiTheme.Ink, new Vector2(600f, 110f), onClick, 46f);
            UiFactory.Place((RectTransform)button.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(600f, 110f));
            return button.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void BuildSlicePanel()
        {
            RectTransform card = BuildCard("SliceCompletePanel", out _slicePanel, 1180f);
            TextMeshProUGUI title = UiFactory.Label(card, "Title", "Range Cleared!", 80f, UiTheme.Ink);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(800f, 110f));
            TextMeshProUGUI subtitle = UiFactory.Label(card, "Subtitle", "Vertical slice complete — tap a level to replay", 38f, UiTheme.InkSoft, style: FontStyles.Normal);
            UiFactory.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(800f, 70f));
            _sliceList = UiFactory.Rect(card, "List");
            UiFactory.Place(_sliceList, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(780f, 760f));
            var list = _sliceList.gameObject.AddComponent<VerticalLayoutGroup>();
            list.spacing = 18f;
            list.childControlHeight = false;
            list.childControlWidth = false;
            list.childForceExpandHeight = false;
            list.childAlignment = TextAnchor.UpperCenter;
            Button again = UiFactory.Button(card, "PlayAgain", "Play again", UiTheme.Gold, UiTheme.Ink, new Vector2(560f, 140f), () =>
            {
                UiFactory.SetVisible(_slicePanel, false);
                _controller.PlayLevel(0);
            }, 60f);
            UiFactory.Place((RectTransform)again.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(560f, 140f));
        }

        // ------------------------------------------------------------------ events

        private void OnLevelStarted(LevelSessionInfo info)
        {
            HideAllPanels();
            LevelData level = _controller.Level;
            _levelLabel.text = $"{level.LevelNumber}  ·  {level.DisplayName}";
            RebuildObjectives();
            RefreshQuiver();
            ShowBanner(string.IsNullOrEmpty(level.Banner) ? "Break the targets!" : level.Banner);
            UpdateSettingsLabels();
        }

        private void OnArrowFired(ArrowFiredInfo info) => RefreshQuiver();

        private void OnObjectiveCleared(ObjectiveInfo info)
        {
            if (info.Index < 0 || info.Index >= _objectiveMarks.Count) return;
            Image icon = _objectiveIcons[info.Index];
            icon.color = new Color(1f, 1f, 1f, 0.35f);
            Image mark = _objectiveMarks[info.Index];
            mark.gameObject.SetActive(true);
            UiTween.Scale(mark.transform, Vector3.one * 1.8f, Vector3.one, 0.3f, Ease.OutBack);
            UiTween.Punch(icon.transform, 0.3f, 0.3f);
        }

        private void OnProtectedLost(ProtectedInfo info)
        {
            if (_protectedMark == null) return;
            _protectedMark.gameObject.SetActive(true);
            UiTween.Punch(_protectedIcon.transform, 0.4f, 0.35f);
        }

        private void OnSoftLock(int remaining) =>
            ShowToast(remaining == 1 ? "1 arrow left" : remaining + " arrows left", 1.6f);

        private void OnStateChanged(GameplayState state)
        {
            if (state == GameplayState.Paused) ShowPanel(_pausePanel);
            else if (_pausePanel.gameObject.activeSelf && state != GameplayState.Paused) UiFactory.SetVisible(_pausePanel, false);

            if (state == GameplayState.Failed && _controller != null && !_controller.Protected.LostThisLevel)
                ShowToast("Out of arrows!", 0.9f);
            if (state == GameplayState.Cooldown || state == GameplayState.Ready) RefreshQuiver();
        }

        private void OnLevelWon(LevelResultInfo info)
        {
            _winTitle.text = info.Stars == 3 ? "Perfect shot!" : "Level Clear!";
            string arrows = info.ArrowsUsed == 1 ? "1 arrow" : info.ArrowsUsed + " arrows";
            _winDetail.text = $"Used {arrows}  ·  Gold par {info.GoldPar}" + (info.Stars < 3 ? $"\n<size=38>Clear it in {info.GoldPar} for ★★★</size>" : "");
            _nextLabel.text = _controller.HasNextLevel ? "Next" : "Finish";
            ShowPanel(_winPanel);
            for (int i = 0; i < 3; i++)
            {
                Image star = _winStars[i];
                bool earned = i < info.Stars;
                star.color = UiTheme.StarEmpty;
                star.transform.localScale = Vector3.one;
                if (!earned) continue;
                int index = i;
                UiTween.Delay(0.25f + i * 0.28f, star, () =>
                {
                    star.color = UiTheme.Gold;
                    UiTween.Scale(star.transform, Vector3.one * 1.6f, Vector3.one, 0.35f, Ease.OutBack);
                    Services.Audio.Play(SfxId.UiStar, 0.9f, 1f + index * 0.12f);
                });
            }
        }

        private void OnLevelFailed(LevelResultInfo info)
        {
            if (info.FailReason == FailReason.ProtectedLost)
            {
                ProtectedKind kind = _controller.Protected.LossInfo.Kind;
                _failTitle.text = kind == ProtectedKind.SleepingFox ? "You woke the fox!" : "The vase broke!";
                _failDetail.text = "Protected objects (purple) must survive.\nFind a shot that sends the fall the other way.";
            }
            else
            {
                _failTitle.text = "Out of arrows";
                _failDetail.text = info.ObjectivesRemaining == 1
                    ? "So close — 1 target left.\nLook for the weak point."
                    : info.ObjectivesRemaining + " targets left.\nLook for a shot that brings them all down.";
            }
            ShowPanel(_failPanel);
        }

        // ------------------------------------------------------------------ helpers

        private void RebuildObjectives()
        {
            foreach (Transform child in _objectiveRow) Destroy(child.gameObject);
            _objectiveIcons.Clear();
            _objectiveMarks.Clear();
            _protectedIcon = null;
            _protectedMark = null;

            int count = _controller.Objectives.Total;
            for (int i = 0; i < count; i++)
            {
                Image icon = UiFactory.Image(_objectiveRow, "Objective" + i, ProceduralTextures.Crest, UiTheme.Red);
                icon.rectTransform.sizeDelta = new Vector2(72f, 72f);
                Image mark = UiFactory.Image(icon.transform, "Check", ProceduralTextures.Circle, UiTheme.Leaf);
                UiFactory.Place(mark.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-6f, 6f), new Vector2(34f, 34f));
                mark.gameObject.SetActive(false);
                _objectiveIcons.Add(icon);
                _objectiveMarks.Add(mark);
            }
            if (_controller.Protected.Items.Count > 0)
            {
                _protectedIcon = UiFactory.Image(_objectiveRow, "Protected", ProceduralTextures.Vase, UiTheme.Purple);
                _protectedIcon.rectTransform.sizeDelta = new Vector2(72f, 72f);
                _protectedMark = UiFactory.Image(_protectedIcon.transform, "Cross", ProceduralTextures.Circle, UiTheme.Red);
                UiFactory.Place(_protectedMark.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-6f, 6f), new Vector2(34f, 34f));
                _protectedMark.gameObject.SetActive(false);
            }
        }

        private void RefreshQuiver()
        {
            QuiverModel quiver = _controller.Quiver;
            int remaining = quiver.Remaining;
            _quiverCount.text = "×" + remaining;
            for (int i = 0; i < _quiverIcons.Count; i++)
            {
                bool visible = i < Mathf.Min(remaining, 5);
                _quiverIcons[i].gameObject.SetActive(visible);
                if (!visible) continue;
                int slot = quiver.Used + i;
                ArrowDefinition def = slot < quiver.Total ? _controller.Spawner.Definition(quiver.TypeAt(slot)) : null;
                Color color = def != null ? def.HudColor : Color.white;
                _quiverIcons[i].color = i == 0 ? color : new Color(color.r, color.g, color.b, 0.6f);
            }
        }

        private void ShowBanner(string text)
        {
            _bannerText.text = text;
            UiFactory.SetVisible(_banner, true);
            _banner.blocksRaycasts = false;
            UiTween.Scale(_banner.transform, Vector3.one * 0.6f, Vector3.one, 0.35f, Ease.OutBack);
            UiTween.Delay(1.7f, _banner, () => UiTween.Fade(_banner, 1f, 0f, 0.35f, 0f, () => _banner.gameObject.SetActive(false)));
        }

        private void ShowToast(string text, float seconds)
        {
            _toastText.text = text;
            UiFactory.SetVisible(_toast, true);
            _toast.blocksRaycasts = false;
            UiTween.Scale(_toast.transform, Vector3.one * 0.7f, Vector3.one, 0.25f, Ease.OutBack);
            UiTween.Delay(seconds, _toast, () => UiTween.Fade(_toast, 1f, 0f, 0.3f, 0f, () => _toast.gameObject.SetActive(false)));
        }

        private void ShowPanel(CanvasGroup panel)
        {
            UiFactory.SetVisible(panel, true);
            UiTween.Fade(panel, 0f, 1f, 0.2f);
            Transform card = panel.transform.Find("Card");
            if (card != null) UiTween.Scale(card, Vector3.one * 0.85f, Vector3.one, 0.3f, Ease.OutBack);
        }

        private void HideAllPanels()
        {
            UiFactory.SetVisible(_winPanel, false);
            UiFactory.SetVisible(_failPanel, false);
            UiFactory.SetVisible(_pausePanel, false);
            UiFactory.SetVisible(_slicePanel, false);
            UiFactory.SetVisible(_toast, false);
            UiFactory.SetVisible(_callout, false);
        }

        private void OnNextClicked()
        {
            if (_controller.HasNextLevel)
            {
                _controller.Next();
                return;
            }
            ShowSliceComplete();
        }

        private void ShowSliceComplete()
        {
            UiFactory.SetVisible(_winPanel, false);
            foreach (Transform child in _sliceList) Destroy(child.gameObject);
            LevelCatalog catalog = _controller.Catalog;
            for (int i = 0; i < catalog.Count; i++)
            {
                LevelData level = catalog.Get(i);
                int stars = Services.Save != null ? ProgressionService.BestStars(Services.Save.Data, level.LevelId) : 0;
                int index = i;
                Button row = UiFactory.Button(_sliceList, "Row" + i, null, UiTheme.ParchmentDark, UiTheme.Ink, new Vector2(760f, 120f), () =>
                {
                    UiFactory.SetVisible(_slicePanel, false);
                    _controller.PlayLevel(index);
                });
                TextMeshProUGUI name = UiFactory.Label(row.transform, "Name", $"{level.LevelNumber}. {level.DisplayName}", 44f, UiTheme.Ink, TextAlignmentOptions.Left);
                UiFactory.Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(460f, 100f));
                for (int s = 0; s < 3; s++)
                {
                    Image star = UiFactory.Image(row.transform, "Star" + s, ProceduralTextures.Star, s < stars ? UiTheme.Gold : UiTheme.StarEmpty);
                    UiFactory.Place(star.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-30f - (2 - s) * 70f, 0f), new Vector2(64f, 64f));
                }
            }
            ShowPanel(_slicePanel);
        }

        // ------------------------------------------------------------------ tutorial

        private void OnGhostHand(bool visible)
        {
            UiFactory.SetVisible(_ghost, visible);
            _ghost.blocksRaycasts = false;
        }

        private void OnCallout(string text, Transform anchor)
        {
            _calloutAnchor = anchor;
            bool visible = !string.IsNullOrEmpty(text);
            if (visible) _calloutText.text = text;
            UiFactory.SetVisible(_callout, visible);
            _callout.blocksRaycasts = false;
            if (visible) UiTween.Scale(_callout.transform, Vector3.one * 0.6f, Vector3.one, 0.35f, Ease.OutBack);
        }

        private void LateUpdate()
        {
            if (_ghost.gameObject.activeSelf)
            {
                float t = Mathf.Repeat(Time.unscaledTime, 1.6f) / 1.6f;
                float drag = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.7f));
                _ghostHand.anchoredPosition = new Vector2(-drag * 90f, -drag * 260f);
                _ghost.alpha = t < 0.85f ? 1f : 1f - (t - 0.85f) / 0.15f;
            }

            if (_callout.gameObject.activeSelf && _calloutAnchor != null)
            {
                Camera cam = Camera.main;
                if (cam == null) return;
                Vector3 screen = cam.WorldToScreenPoint(_calloutAnchor.position);
                var canvasRect = (RectTransform)_safe.parent;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
                var rect = (RectTransform)_callout.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.SetParent(canvasRect, false);
                rect.anchoredPosition = local + new Vector2(0f, 90f + Mathf.Sin(Time.unscaledTime * 4f) * 8f);
            }
        }

        // ------------------------------------------------------------------ settings

        private void ToggleSound()
        {
            SettingsData s = Services.Save.Data.settings;
            s.sfx = !s.sfx;
            Services.Audio.SfxVolume = s.sfx ? 1f : 0f;
            Services.Save.Save();
            UpdateSettingsLabels();
        }

        private void ToggleMusic()
        {
            SettingsData s = Services.Save.Data.settings;
            s.music = !s.music;
            Services.Audio.MusicVolume = s.music ? 0.55f : 0f;
            Services.Save.Save();
            UpdateSettingsLabels();
        }

        private void ToggleHaptics()
        {
            SettingsData s = Services.Save.Data.settings;
            s.haptics = !s.haptics;
            Services.Haptics.Enabled = s.haptics;
            Services.Save.Save();
            UpdateSettingsLabels();
        }

        private void UpdateSettingsLabels()
        {
            if (Services.Save == null) return;
            SettingsData s = Services.Save.Data.settings;
            _soundLabel.text = "Sound: " + (s.sfx ? "On" : "Off");
            _musicLabel.text = "Music: " + (s.music ? "On" : "Off");
            _hapticsLabel.text = "Haptics: " + (s.haptics ? "On" : "Off");
        }
    }
}
