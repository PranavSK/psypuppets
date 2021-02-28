using System;
using System.Collections;
using PsyPuppets.Gameplay.Characters;
using PsyPuppets.Gameplay.Characters.Controllers;
using UnityEngine;

namespace PsyPuppets.Gameplay.Props
{
    public class CharacterSpawner : MonoBehaviour
    {
        [SerializeField]
        private PsyCharacter _characterPrefab;

        [SerializeField]
        private Transform _spawnLocation;

        [SerializeField]
        private bool _isSpawnAtStart = false;

        private IAbilityController _spawnedCharacter = null;
        private bool _canSpawn = true;

        private void Start()
        {
            if (_isSpawnAtStart)
            {
                Spawn();
                _canSpawn = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && _canSpawn)
            {
                Spawn();
                _canSpawn = false;
            }
        }

        private void Spawn()
        {
            var character = Instantiate(_characterPrefab, _spawnLocation.position, Quaternion.identity);
            _spawnedCharacter = character.GetComponent<IAbilityController>();
            _spawnedCharacter.Died += OnDead;
        }

        private void OnDead()
        {
            _spawnedCharacter = null;
            StartCoroutine(Delay());
        }

        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.0f);

            if (_isSpawnAtStart)
            {
                Spawn();
            }
            else
            {
                _canSpawn = true;
            }
        }
    }
}