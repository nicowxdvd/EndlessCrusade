using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class CameraFollowHero : MonoBehaviour
    {
        Transform hero;

        void OnEnable()
        {
            EventBus<HeroSpawned>.Subscribe(OnHeroSpawned);
        }

        void OnDisable()
        {
            EventBus<HeroSpawned>.Unsubscribe(OnHeroSpawned);
        }

        void LateUpdate()
        {
            if (hero == null)
                return;
            var position = transform.position;
            position.x = hero.position.x;
            transform.position = position;
        }

        void OnHeroSpawned(HeroSpawned evt)
        {
            hero = evt.Hero.transform;
        }
    }
}
