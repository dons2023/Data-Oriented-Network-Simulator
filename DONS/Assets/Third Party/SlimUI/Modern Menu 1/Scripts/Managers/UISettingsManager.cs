using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace SlimUI.ModernMenu{
	public class UISettingsManager : MonoBehaviour {

		public enum Platform {Desktop, Mobile};
		public Platform platform;
		// toggle buttons
		[Header("MOBILE SETTINGS")]
		public GameObject mobileSFXtext;
		public GameObject mobileMusictext;
		public GameObject mobileShadowofftextLINE;
		public GameObject mobileShadowlowtextLINE;
		public GameObject mobileShadowhightextLINE;

		[Header("VIDEO SETTINGS")]
		public GameObject fullscreentext;
		public GameObject ambientocclusiontext;
		public GameObject shadowofftextLINE;
		public GameObject shadowlowtextLINE;
		public GameObject shadowhightextLINE;
		public GameObject aaofftextLINE;
		public GameObject aa2xtextLINE;
		public GameObject aa4xtextLINE;
		public GameObject aa8xtextLINE;
		public GameObject vsynctext;
		public GameObject motionblurtext;
		public GameObject texturelowtextLINE;
		public GameObject texturemedtextLINE;
		public GameObject texturehightextLINE;
		public GameObject cameraeffectstext; 

		[Header("GAME SETTINGS")]
		public GameObject showhudtext;
		public GameObject tooltipstext;

        [Header("SETTINGS-Type")]
        public GameObject typeAbitext;
		public GameObject typeAbitextLINE;
		public GameObject typeGEANTtext;
		public GameObject typeGEANTtextLINE;
        public GameObject typeFatreetext;
        public GameObject typeFatreetextLINE;

        [Header("CONTROLS SETTINGS")]
		public GameObject invertmousetext;

		// sliders
		public GameObject FlowNumSlider;
		public GameObject ReceiverNumSlider;
		public GameObject ReceiverRangeSlider;
		public GameObject SpawnerLoadSlider;
        public GameObject SpawnerLoadRange;

        //private float sliderValue = 0.0f;
        private float FlowNumSliderValue = 0.0f;
		private float ReceiverNumSliderValue = 0.0f;
		private float ReceiverRangeSliderValue = 0.0f;
        private float SpawnerLoadSliderValue = 0.0f;
        private float SpawnerLoadRangeValue = 0.0f;

        public void  Start (){

            #region Ori
            // check difficulty
            //         if (PlayerPrefs.GetInt("NormalDifficulty") == 1){
            //	difficultynormaltextLINE.gameObject.SetActive(true);
            //	difficultyhardcoretextLINE.gameObject.SetActive(false);
            //}
            //else
            //{
            //	difficultyhardcoretextLINE.gameObject.SetActive(true);
            //	difficultynormaltextLINE.gameObject.SetActive(false);
            //}

            // check slider values
         

            // check full screen
            if (Screen.fullScreen == true){
				fullscreentext.GetComponent<TMP_Text>().text = "on";
			}
			else if(Screen.fullScreen == false){
				fullscreentext.GetComponent<TMP_Text>().text = "off";
			}

			// check hud value
			if(PlayerPrefs.GetInt("ShowHUD")==0){
				showhudtext.GetComponent<TMP_Text>().text = "off";
			}
			else{
				showhudtext.GetComponent<TMP_Text>().text = "on";
			}

			// check tool tip value
			if(PlayerPrefs.GetInt("ToolTips")==0){
				tooltipstext.GetComponent<TMP_Text>().text = "off";
			}
			else{
				tooltipstext.GetComponent<TMP_Text>().text = "on";
			}

			// check shadow distance/enabled
			if(platform == Platform.Desktop){
				if(PlayerPrefs.GetInt("Shadows") == 0){
					QualitySettings.shadowCascades = 0;
					QualitySettings.shadowDistance = 0;
					shadowofftextLINE.gameObject.SetActive(true);
					shadowlowtextLINE.gameObject.SetActive(false);
					shadowhightextLINE.gameObject.SetActive(false);
				}
				else if(PlayerPrefs.GetInt("Shadows") == 1){
					QualitySettings.shadowCascades = 2;
					QualitySettings.shadowDistance = 75;
					shadowofftextLINE.gameObject.SetActive(false);
					shadowlowtextLINE.gameObject.SetActive(true);
					shadowhightextLINE.gameObject.SetActive(false);
				}
				else if(PlayerPrefs.GetInt("Shadows") == 2){
					QualitySettings.shadowCascades = 4;
					QualitySettings.shadowDistance = 500;
					shadowofftextLINE.gameObject.SetActive(false);
					shadowlowtextLINE.gameObject.SetActive(false);
					shadowhightextLINE.gameObject.SetActive(true);
				}
			}else if(platform == Platform.Mobile){
				if(PlayerPrefs.GetInt("MobileShadows") == 0){
					QualitySettings.shadowCascades = 0;
					QualitySettings.shadowDistance = 0;
					mobileShadowofftextLINE.gameObject.SetActive(true);
					mobileShadowlowtextLINE.gameObject.SetActive(false);
					mobileShadowhightextLINE.gameObject.SetActive(false);
				}
				else if(PlayerPrefs.GetInt("MobileShadows") == 1){
					QualitySettings.shadowCascades = 2;
					QualitySettings.shadowDistance = 75;
					mobileShadowofftextLINE.gameObject.SetActive(false);
					mobileShadowlowtextLINE.gameObject.SetActive(true);
					mobileShadowhightextLINE.gameObject.SetActive(false);
				}
				else if(PlayerPrefs.GetInt("MobileShadows") == 2){
					QualitySettings.shadowCascades = 4;
					QualitySettings.shadowDistance = 100;
					mobileShadowofftextLINE.gameObject.SetActive(false);
					mobileShadowlowtextLINE.gameObject.SetActive(false);
					mobileShadowhightextLINE.gameObject.SetActive(true);
				}
			}


			// check vsync
			if(QualitySettings.vSyncCount == 0){
				vsynctext.GetComponent<TMP_Text>().text = "off";
			}
			else if(QualitySettings.vSyncCount == 1){
				vsynctext.GetComponent<TMP_Text>().text = "on";
			}

			// check mouse inverse
			if(PlayerPrefs.GetInt("Inverted")==0){
				invertmousetext.GetComponent<TMP_Text>().text = "off";
			}
			else if(PlayerPrefs.GetInt("Inverted")==1){
				invertmousetext.GetComponent<TMP_Text>().text = "on";
			}

			// check motion blur
			if(PlayerPrefs.GetInt("MotionBlur")==0){
				motionblurtext.GetComponent<TMP_Text>().text = "off";
			}
			else if(PlayerPrefs.GetInt("MotionBlur")==1){
				motionblurtext.GetComponent<TMP_Text>().text = "on";
			}

			// check ambient occlusion
			if(PlayerPrefs.GetInt("AmbientOcclusion")==0){
				ambientocclusiontext.GetComponent<TMP_Text>().text = "off";
			}
			else if(PlayerPrefs.GetInt("AmbientOcclusion")==1){
				ambientocclusiontext.GetComponent<TMP_Text>().text = "on";
			}

			// check texture quality
			if(PlayerPrefs.GetInt("Textures") == 0){
				QualitySettings.masterTextureLimit = 2;
				texturelowtextLINE.gameObject.SetActive(true);
				texturemedtextLINE.gameObject.SetActive(false);
				texturehightextLINE.gameObject.SetActive(false);
			}
			else if(PlayerPrefs.GetInt("Textures") == 1){
				QualitySettings.masterTextureLimit = 1;
				texturelowtextLINE.gameObject.SetActive(false);
				texturemedtextLINE.gameObject.SetActive(true);
				texturehightextLINE.gameObject.SetActive(false);
			}
			else if(PlayerPrefs.GetInt("Textures") == 2){
				QualitySettings.masterTextureLimit = 0;
				texturelowtextLINE.gameObject.SetActive(false);
				texturemedtextLINE.gameObject.SetActive(false);
				texturehightextLINE.gameObject.SetActive(true);
			}

			#endregion

			#region topo

			//GlobalSetting.Instance.Data.TopoType
			// check difficulty
			if (GlobalSetting.Instance.Data.TopoType == -1)
			{
                typeAbitextLINE.gameObject.SetActive(true);
                typeGEANTtextLINE.gameObject.SetActive(false);
                typeFatreetextLINE.gameObject.SetActive(false);
            }
			else if (GlobalSetting.Instance.Data.TopoType == -2)
			{
                typeAbitextLINE.gameObject.SetActive(false);
                typeGEANTtextLINE.gameObject.SetActive(true);
                typeFatreetextLINE.gameObject.SetActive(false);
            }
			else
			{
                typeAbitextLINE.gameObject.SetActive(false);
                typeGEANTtextLINE.gameObject.SetActive(false);
                typeFatreetextLINE.gameObject.SetActive(true);
            }

			#endregion

			#region  Flow
			//FlowNumSliderValue=....
			FlowNumSlider.GetComponent<Slider>().value = GlobalSetting.Instance.Data.FlowNumAtTime;
            ReceiverNumSlider.GetComponent<Slider>().value = GlobalSetting.Instance.Data.Receiver_RX_nums;
            ReceiverRangeSlider.GetComponent<Slider>().value = GlobalSetting.Instance.Data.Receiver_RX_nums_range;
            SpawnerLoadSlider.GetComponent<Slider>().value = GlobalSetting.Instance.Data.Sender_load;
            SpawnerLoadRange.GetComponent<Slider>().value = GlobalSetting.Instance.Data.Sender_load_range;
            
            #endregion
        }

        public void Update (){
            //sliderValue = musicSlider.GetComponent<Slider>().value;
            FlowNumSliderValue = FlowNumSlider.GetComponent<Slider>().value;
            ReceiverNumSliderValue = ReceiverNumSlider.GetComponent<Slider>().value;
            ReceiverRangeSliderValue = ReceiverRangeSlider.GetComponent<Slider>().value;
            SpawnerLoadSliderValue = SpawnerLoadSlider.GetComponent<Slider>().value;
            SpawnerLoadRangeValue = SpawnerLoadRange.GetComponent<Slider>().value;
        }

        #region topoType

        public void SetAbiType()
        {
            typeAbitextLINE.gameObject.SetActive(true);
            typeGEANTtextLINE.gameObject.SetActive(false);
            typeFatreetextLINE.gameObject.SetActive(false);
			GlobalSetting.Instance.Data.TopoType = -1;
			Debug.Log("SetAbiType");
        }

        public void SetGEANTType()
        {
            typeAbitextLINE.gameObject.SetActive(false);
            typeGEANTtextLINE.gameObject.SetActive(true);
            typeFatreetextLINE.gameObject.SetActive(false);
            GlobalSetting.Instance.Data.TopoType = -2;
            Debug.Log("SetGEANTType");
        }

        public void SetFattreeType()
        {
            typeAbitextLINE.gameObject.SetActive(false);
            typeGEANTtextLINE.gameObject.SetActive(false);
            typeFatreetextLINE.gameObject.SetActive(true);
            GlobalSetting.Instance.Data.TopoType = 0;
			GlobalSetting.Instance.Data.Fattree_K = 4;
            Debug.Log("SetFattreeType");
        }



        #endregion

        public void FullScreen (){
			Screen.fullScreen = !Screen.fullScreen;

			if(Screen.fullScreen == true){
				fullscreentext.GetComponent<TMP_Text>().text = "on";
			}
			else if(Screen.fullScreen == false){
				fullscreentext.GetComponent<TMP_Text>().text = "off";
			}
		}
        #region Flow
        public void FlowNumSliderChanged(){
            ////PlayerPrefs.SetFloat("MusicVolume", sliderValue);
            //PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
            GlobalSetting.Instance.Data.FlowNumAtTime = (int)FlowNumSlider.GetComponent<Slider>().value;
        }

		public void ReceiverNumSliderChanged(){
            //PlayerPrefs.SetFloat("XSensitivity", FlowNumSliderValue);
            GlobalSetting.Instance.Data.Receiver_RX_nums = (int)ReceiverNumSlider.GetComponent<Slider>().value;
        }

		public void ReceiverRangeSliderChanged(){
            //PlayerPrefs.SetFloat("YSensitivity", ReceiverRangeSliderValue);
            GlobalSetting.Instance.Data.Receiver_RX_nums_range = (int)ReceiverRangeSlider.GetComponent<Slider>().value;
        }

		public void SpawnerLoadSliderChanged(){
            //PlayerPrefs.SetFloat("MouseSmoothing", SpawnerLoadSliderValue);
            //Debug.Log(PlayerPrefs.GetFloat("MouseSmoothing"));
            GlobalSetting.Instance.Data.Sender_load = (int)SpawnerLoadSlider.GetComponent<Slider>().value;
        }

        public void SpawnerLoadRangeSliderChanged()
        {
            //PlayerPrefs.SetFloat("MouseSmoothing", SpawnerLoadRangeValue);
            //Debug.Log(PlayerPrefs.GetFloat("MouseSmoothing"));
            GlobalSetting.Instance.Data.Sender_load_range = (int)SpawnerLoadRange.GetComponent<Slider>().value;
        }

        #endregion

        // the playerprefs variable that is checked to enable hud while in game
        public void ShowHUD (){
			if(PlayerPrefs.GetInt("ShowHUD")==0){
				PlayerPrefs.SetInt("ShowHUD",1);
				showhudtext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("ShowHUD")==1){
				PlayerPrefs.SetInt("ShowHUD",0);
				showhudtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		// the playerprefs variable that is checked to enable mobile sfx while in game
		public void MobileSFXMute (){
			if(PlayerPrefs.GetInt("Mobile_MuteSfx")==0){
				PlayerPrefs.SetInt("Mobile_MuteSfx",1);
				mobileSFXtext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("Mobile_MuteSfx")==1){
				PlayerPrefs.SetInt("Mobile_MuteSfx",0);
				mobileSFXtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void MobileMusicMute (){
			if(PlayerPrefs.GetInt("Mobile_MuteMusic")==0){
				PlayerPrefs.SetInt("Mobile_MuteMusic",1);
				mobileMusictext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("Mobile_MuteMusic")==1){
				PlayerPrefs.SetInt("Mobile_MuteMusic",0);
				mobileMusictext.GetComponent<TMP_Text>().text = "off";
			}
		}

		// show tool tips like: 'How to Play' control pop ups
		public void ToolTips (){
			if(PlayerPrefs.GetInt("ToolTips")==0){
				PlayerPrefs.SetInt("ToolTips",1);
				tooltipstext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("ToolTips")==1){
				PlayerPrefs.SetInt("ToolTips",0);
				tooltipstext.GetComponent<TMP_Text>().text = "off";
			}
		}



		public void ShadowsOff (){
			PlayerPrefs.SetInt("Shadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			shadowofftextLINE.gameObject.SetActive(true);
			shadowlowtextLINE.gameObject.SetActive(false);
			shadowhightextLINE.gameObject.SetActive(false);
		}

		public void ShadowsLow (){
			PlayerPrefs.SetInt("Shadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			shadowofftextLINE.gameObject.SetActive(false);
			shadowlowtextLINE.gameObject.SetActive(true);
			shadowhightextLINE.gameObject.SetActive(false);
		}

		public void ShadowsHigh (){
			PlayerPrefs.SetInt("Shadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			shadowofftextLINE.gameObject.SetActive(false);
			shadowlowtextLINE.gameObject.SetActive(false);
			shadowhightextLINE.gameObject.SetActive(true);
		}

		public void MobileShadowsOff (){
			PlayerPrefs.SetInt("MobileShadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			mobileShadowofftextLINE.gameObject.SetActive(true);
			mobileShadowlowtextLINE.gameObject.SetActive(false);
			mobileShadowhightextLINE.gameObject.SetActive(false);
		}

		public void MobileShadowsLow (){
			PlayerPrefs.SetInt("MobileShadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			mobileShadowofftextLINE.gameObject.SetActive(false);
			mobileShadowlowtextLINE.gameObject.SetActive(true);
			mobileShadowhightextLINE.gameObject.SetActive(false);
		}

		public void MobileShadowsHigh (){
			PlayerPrefs.SetInt("MobileShadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			mobileShadowofftextLINE.gameObject.SetActive(false);
			mobileShadowlowtextLINE.gameObject.SetActive(false);
			mobileShadowhightextLINE.gameObject.SetActive(true);
		}

		public void vsync (){
			if(QualitySettings.vSyncCount == 0){
				QualitySettings.vSyncCount = 1;
				vsynctext.GetComponent<TMP_Text>().text = "on";
			}
			else if(QualitySettings.vSyncCount == 1){
				QualitySettings.vSyncCount = 0;
				vsynctext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void InvertMouse (){
			if(PlayerPrefs.GetInt("Inverted")==0){
				PlayerPrefs.SetInt("Inverted",1);
				invertmousetext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("Inverted")==1){
				PlayerPrefs.SetInt("Inverted",0);
				invertmousetext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void MotionBlur (){
			if(PlayerPrefs.GetInt("MotionBlur")==0){
				PlayerPrefs.SetInt("MotionBlur",1);
				motionblurtext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("MotionBlur")==1){
				PlayerPrefs.SetInt("MotionBlur",0);
				motionblurtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void AmbientOcclusion (){
			if(PlayerPrefs.GetInt("AmbientOcclusion")==0){
				PlayerPrefs.SetInt("AmbientOcclusion",1);
				ambientocclusiontext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("AmbientOcclusion")==1){
				PlayerPrefs.SetInt("AmbientOcclusion",0);
				ambientocclusiontext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void CameraEffects (){
			if(PlayerPrefs.GetInt("CameraEffects")==0){
				PlayerPrefs.SetInt("CameraEffects",1);
				cameraeffectstext.GetComponent<TMP_Text>().text = "on";
			}
			else if(PlayerPrefs.GetInt("CameraEffects")==1){
				PlayerPrefs.SetInt("CameraEffects",0);
				cameraeffectstext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void TexturesLow (){
			PlayerPrefs.SetInt("Textures",0);
			QualitySettings.masterTextureLimit = 2;
			texturelowtextLINE.gameObject.SetActive(true);
			texturemedtextLINE.gameObject.SetActive(false);
			texturehightextLINE.gameObject.SetActive(false);
		}

		public void TexturesMed (){
			PlayerPrefs.SetInt("Textures",1);
			QualitySettings.masterTextureLimit = 1;
			texturelowtextLINE.gameObject.SetActive(false);
			texturemedtextLINE.gameObject.SetActive(true);
			texturehightextLINE.gameObject.SetActive(false);
		}

		public void TexturesHigh (){
			PlayerPrefs.SetInt("Textures",2);
			QualitySettings.masterTextureLimit = 0;
			texturelowtextLINE.gameObject.SetActive(false);
			texturemedtextLINE.gameObject.SetActive(false);
			texturehightextLINE.gameObject.SetActive(true);
		}
	}
}