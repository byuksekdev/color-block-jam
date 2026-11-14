using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorBlockJam.Game.UI
{
    /// <summary>Makes a button shrink a little while it is pressed, whether or not the button does anything.</summary>
    public sealed class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float PressedScale = 0.9f;
        private const float Seconds = 0.08f;

        public void OnPointerDown(PointerEventData eventData)
        {
            ScaleTo(PressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ScaleTo(1f);
        }

        private void OnDisable()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
        }

        private void ScaleTo(float scale)
        {
            transform.DOKill();
            transform.DOScale(scale, Seconds).SetLink(gameObject);
        }
    }
}
