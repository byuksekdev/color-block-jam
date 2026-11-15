using DG.Tweening;
using UnityEngine;

namespace ColorBlockJam.Game.UI
{
    /// <summary>A panel shown over the screen behind it. It pops in when shown.</summary>
    public abstract class Popup : MonoBehaviour
    {
        [SerializeField] private UiMotion _motion;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private CanvasGroup _canvasGroup;

        public void Show()
        {
            gameObject.SetActive(true);
            StopTweens();

            _panel.localScale = Vector3.one * _motion.PopupStartScale;
            _canvasGroup.alpha = 0f;
            _panel.DOScale(1f, _motion.PopupSeconds).SetEase(Ease.OutBack).SetLink(gameObject);
            _canvasGroup.DOFade(1f, _motion.PopupSeconds).SetLink(gameObject);
        }

        public void Hide()
        {
            StopTweens();
            gameObject.SetActive(false);
        }

        private void StopTweens()
        {
            _panel.DOKill();
            _canvasGroup.DOKill();
        }
    }
}
