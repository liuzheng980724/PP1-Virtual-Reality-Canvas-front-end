using System;
using System.Collections;
using UnityEngine;

namespace Snobal.CameraRigSystems_0_0
{
	public class FadeTransitionControl : MonoBehaviour
	{
        private enum FadeType { ToOpaque, ToClear }

        public enum State { Clear, Opaque, FadingClear, FadingOpaque}
		private State currentState = default(State);
                
        [SerializeField]
		private bool initiallyOpaque = false;

		[Header("Properties")]
		[SerializeField]
		private AnimationCurve fadeAnimCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [SerializeField]
        private string FadeTransitionSphereResourceName = "FadeTransitionSpherePrefab";

        private GameObject fadeSphere;
        private Renderer fadeQuad;
		private float transitionDuration = 0.5f;
		private float fadeWaitDuration = 0.25f;

        private void Awake()
        {
            var fadeSpherePrefab = Resources.Load<GameObject>(FadeTransitionSphereResourceName);

            if (fadeSpherePrefab == null)
                throw new UnityException($"Cannot find {FadeTransitionSphereResourceName} gameobject resource");

            fadeSphere = GameObject.Instantiate(fadeSpherePrefab, CameraRig.RigTransforms.Head);
            fadeSphere.name = "FADE TRANSITION SPHERE";

            fadeQuad = fadeSphere.GetComponent<MeshRenderer>();

            if (initiallyOpaque)
            {
				fadeQuad.material.color = Color.black;
				currentState = State.Opaque;
            }
			else
            {
				fadeQuad.material.color = Color.clear;
				currentState = State.Clear;
			}
        }

		/// <summary> Starts the black screen fading in to clear</summary>
		/// <param name="_fadeResolvedCallback"> Optional callback to trigger when fading completes </param>
		public void StartFadeClear(Action _fadeResolvedCallback = null)
		{
            if (fadeQuad == null)
                throw new UnityException("Fade quad renderer missing");

            switch (currentState)
            {
                case State.Opaque:
                    StartCoroutine(CoroTransition(FadeType.ToClear, _fadeResolvedCallback));
                    break;
                case State.FadingOpaque:
                    StartCoroutine(WaitUntilTrue(() => (currentState == State.Opaque), () => {
                        StartCoroutine(CoroTransition(FadeType.ToClear, _fadeResolvedCallback));
                    }));
                    break;
                case State.Clear: // if already clear do nothing, just invoke the callback
                    _fadeResolvedCallback?.Invoke();
                    break;
                case State.FadingClear:
                    StartCoroutine(WaitUntilTrue(() => (currentState == State.Clear), () => {
                        _fadeResolvedCallback?.Invoke();
                    }));
                    break;                
            }
        }

        /// <summary> Starts clear screen fading out to black </summary>
        /// <param name="_fadeResolvedCallback"> Optional callback to trigger when fading completes </param>
        public void StartFadeOpaque(Action _fadeResolvedCallback = null)
		{
            if (fadeQuad == null)
                throw new UnityException("Fade quad renderer missing");

            switch (currentState)
            {
                case State.Clear: // start the fade to opaque
                    StartCoroutine(CoroTransition(FadeType.ToOpaque, _fadeResolvedCallback));
                    break;
                case State.FadingClear: // if fading clear, then wait until clear, then fade to opaque immediately
                    StartCoroutine(WaitUntilTrue(() => (currentState == State.Clear), () =>
                    {
                        StartCoroutine(CoroTransition(FadeType.ToOpaque, _fadeResolvedCallback));
                    }));
                    break;
                case State.Opaque: // if already opaque do nothing, just invoke the callback
                    _fadeResolvedCallback?.Invoke();
                    break;
                case State.FadingOpaque: // if fading opaque already, just wait for the fade to complete then invoke the callback
                    StartCoroutine(WaitUntilTrue(() => (currentState == State.Opaque), () =>
                    {
                        _fadeResolvedCallback?.Invoke();
                    }));
                    break;                
            }
        }

        IEnumerator WaitUntilTrue (Func<bool> predicate, Action callback)
        {
			yield return new WaitUntil(predicate);
			callback?.Invoke();
		}        

		IEnumerator CoroTransition(FadeType fadeType, Action OnComplete)
        {
            float elapsedTime = 0;

			Color startColour = Color.clear;
			Color endColour = Color.clear;
			State endState = default(State);

			switch (fadeType)
            {
                case FadeType.ToOpaque:
					startColour = Color.clear;
					endColour = Color.black;
					currentState = State.FadingOpaque;
					endState = State.Opaque;
					break;
                case FadeType.ToClear:
					startColour = Color.black;
					endColour = Color.clear;
					currentState = State.FadingClear;
					endState = State.Clear;
					break;
            }

			while (elapsedTime < transitionDuration)
			{
				fadeQuad.material.color = Color.Lerp(startColour, endColour, fadeAnimCurve.Evaluate((elapsedTime / transitionDuration)));
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			// Make sure we got there
			fadeQuad.material.color = endColour;

			yield return new WaitForSeconds(fadeWaitDuration);

			currentState = endState;
			OnComplete?.Invoke();
		}
    }
}