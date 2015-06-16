using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GroundType
{
		Undefined,
		Brick,
		SolidBrick,
		IndestructibleBrick,
		Nitro
}

public class GroundElement : MonoBehaviour
{

		public int ElementIndex;
		// public AudioClip ExplosionAudio, TapAudio;
		public GroundType CurrentGroundType = GroundType.Undefined;
		public int Life = 0;
		public Sprite[] Sprites;
		public GameObject ExplosionPrefab;
		private SpriteRenderer _spriteManager;

    #region MONO BEHAVIOUR METHODS

		void Awake ()
		{
				_spriteManager = this.GetComponent<SpriteRenderer> ();
		}

		void Start ()
		{				
				switch (CurrentGroundType) {
				case GroundType.Brick:
						Life = 1;
						_spriteManager.sprite = Sprites [0];
						break;
				case GroundType.SolidBrick:
						Life = 2;
						_spriteManager.sprite = Sprites [1];
						break;

				case GroundType.Nitro:
						Life = 1;
						_spriteManager.sprite = Sprites [4];
						break; 
				case GroundType.IndestructibleBrick:
						_spriteManager.sprite = Sprites [2];
						break;
				}

		}

    #endregion

		void UpdateSprite (int? spriteSelection = null)
		{
				if (Sprites != null && _spriteManager != null) {
						if (spriteSelection != null) {
								_spriteManager.sprite = Sprites [(int)spriteSelection];
						} else {
								if (Life == 1) {
										_spriteManager.sprite = Sprites [3];
								} else if (Life == 2) {
										_spriteManager.sprite = Sprites [1];
								}
						}
				}
		}

		public void Tap ()
		{
				if (CurrentGroundType == GroundType.IndestructibleBrick) {
						return;
				}

				if (CurrentGroundType == GroundType.Nitro) {
						MortalExplosion ();
						return;
				}

				// var minerChar = GameObject.Find("MinerCharacter");
				// var audio = minerChar.GetComponent<AudioSource>();

				--Life;


				if (Life <= 0) {
						/*
						var explo = Resources.Load ("Music/explosion") as AudioClip;
						if (explo != null) {
								audio.clip = explo as AudioClip;
						}		
						*/
						Explosion ();
				} else {
						/*
						var explo = Resources.Load ("Music/explosion_3") as AudioClip;
						if (explo != null) {
								audio.clip = explo;
						}
						*/
						UpdateSprite ();
				}

				// audio.Play ();

		}

		public void MortalExplosion ()
		{
				var explosionPrefab = Resources.Load ("ExplosionPrefab") as GameObject;
				Instantiate (explosionPrefab, this.transform.position, Quaternion.identity);
				Destroy (this.gameObject);
		}

		public void Explosion ()
		{
				Instantiate (ExplosionPrefab, this.transform.position, Quaternion.identity);
				Destroy (this.gameObject);
		}

}

