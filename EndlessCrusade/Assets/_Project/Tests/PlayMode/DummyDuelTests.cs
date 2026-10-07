using System.Collections;
using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class DummyDuelTests
    {
        static GameObject Spawn(Team team, float x)
        {
            var go = new GameObject(team.ToString());
            go.SetActive(false);
            go.AddComponent<HealthComponent>().Initialize(30, team);
            go.AddComponent<MovementComponent>();
            go.AddComponent<AttackComponent>().cooldown = 0.2f;
            go.AddComponent<EntityController>().hurtDuration = 0.05f;
            go.AddComponent<DummyBrain>();
            go.transform.position = new Vector3(x, 0f, 0f);
            go.SetActive(true);
            return go;
        }

        [UnityTest]
        public IEnumerator TwoOppositeDummies_ApproachAndOneDies()
        {
            var a = Spawn(Team.Player, -4f);
            var b = Spawn(Team.Enemy, 4f);
            var ha = a.GetComponent<HealthComponent>();
            var hb = b.GetComponent<HealthComponent>();

            var timeout = Time.time + 20f;
            while (ha.IsAlive && hb.IsAlive && Time.time < timeout)
                yield return null;

            Assert.IsFalse(ha.IsAlive && hb.IsAlive);
            var dead = ha.IsAlive ? b : a;
            Assert.AreEqual(EntityState.Dead, dead.GetComponent<EntityController>().State);
            var pos = dead.transform.position.x;
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(pos, dead.transform.position.x, 0.0001f);

            Object.Destroy(a);
            Object.Destroy(b);
        }
    }
}
