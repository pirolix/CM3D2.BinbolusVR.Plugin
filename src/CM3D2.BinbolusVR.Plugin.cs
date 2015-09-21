using System;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.BinbolusVR
{
    [PluginFilter( "CM3D2x64" ),
     PluginFilter( "CM3D2x86" ),
     PluginName( "BinbolusVR" ), PluginVersion("0.0.0.1")]

    public class BinbolusVR : PluginBase
    {
        private bool        m_bOculusVR = false;
        private bool        m_AllowUpdate = false;

        private Camera      m_CameraL;
        private Camera      m_CameraR;
        private bool        m_StereoPower;
        private bool        m_StereoMode;

        private string      m_cfgKeyStereoPower = "k";
        private string      m_cfgKeyStereoMode  = "l";
        private float       m_cfgParallaxScale  = 0.2f;

        /// <summary>プラグインが初期化されたタイミングで呼ばれる</summary>
        public void Awake()
        {
            // VRモードでは動作しない
            m_bOculusVR = Application.dataPath.Contains( "CM3D2VRx64" );
            if( m_bOculusVR ) {
                Console.WriteLine( "{0}: Occuls Rift is not Support.", this.GetPluginName());
                return;
            }
            GameObject.DontDestroyOnLoad( this );
            GetPluginPreferences();
        }

        /// <summary>ゲームレベルが変化した際に呼ばれる</summary>
        public void OnLevelWasLoaded(int level)
        {
            m_AllowUpdate = false;
            if( m_bOculusVR)
                return;
            // level: 5)エディット 14)夜伽
            if( level == 5 || level == 14 )
            {
                // 左目用カメラ
                m_CameraL = (new GameObject( "ParallaxCameraL" )).AddComponent<Camera>();
                m_CameraL.CopyFrom(Camera.main);
                m_CameraL.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
                // 右目用カメラ
                m_CameraR = (new GameObject( "ParallaxCameraR" )).AddComponent<Camera>();
                m_CameraR.CopyFrom(Camera.main);
                m_CameraR.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);

                SetStereoPower( false );
                SetStereoMode( true );

                m_AllowUpdate = true;
            }
        }

        /// <summary>.ini ファイルからプラグイン設定を読み込む</summary>
        private void GetPluginPreferences()
        {
            // http://docs.unity3d.com/Manual/ConventionalGameInput.html
            m_cfgKeyStereoPower = GetPreferences( "Config", "TogglePower", m_cfgKeyStereoPower ).ToLower();
            Console.WriteLine( "{0}: Config::ToggleKeyPower = {1}", this.GetPluginName(), m_cfgKeyStereoPower );
            m_cfgKeyStereoMode = GetPreferences( "Config", "ToggleMode", m_cfgKeyStereoMode ).ToLower();
            Console.WriteLine( "{0}: Config::ToggleKeyMode = {1}", this.GetPluginName(), m_cfgKeyStereoMode );
            m_cfgParallaxScale = GetPreferences( "Config", "ParallaxScale", m_cfgParallaxScale );
            Console.WriteLine( "{0}: Config::ParallaxScale = {1}", this.GetPluginName(), m_cfgParallaxScale );
        }

        /// <summary>画面を更新する</summary>
        public void LateUpdate()
        {
            if( !m_AllowUpdate )
                return;

            Maid maid = GameMain.Instance.CharacterMgr.GetMaid( 0 );
            if( maid == null )
                return;

            // 視差ベクトル
            Vector2 v = GameMain.Instance.MainCamera.GetAroundAngle();
            //Console.WriteLine( "AroundAngle x={0} Y={1}", v.x, v.y );
            Vector3 parallax = (new Vector3( Mathf.Cos( v.x * Mathf.Deg2Rad ), 0.0f, -Mathf.Sin( v.x * Mathf.Deg2Rad )))
                    * m_cfgParallaxScale
                    * ( m_StereoMode ? -1 : 1 );
                     
            if( m_StereoPower ) {
                // MainCamera が狙っている target:Vector3
                Vector3 target = GameMain.Instance.MainCamera.GetTargetPos();
                // カメラの場所を視差分だけずらして
                m_CameraL.gameObject.transform.position = Camera.main.transform.position - parallax;
                m_CameraR.gameObject.transform.position = Camera.main.transform.position + parallax;
                // target を狙いなおす
                m_CameraL.gameObject.transform.LookAt( target, maid.transform.up );
                m_CameraR.gameObject.transform.LookAt( target, maid.transform.up );
            }
 
            // キー入力で切替える：オン/オフ
            if( Input.GetKeyDown( m_cfgKeyStereoPower )) {
                m_StereoPower = !m_StereoPower;
                this.SetStereoPower( m_StereoPower );
            }

            // キー入力で切替える：平行法/交差法
            if( Input.GetKeyDown( m_cfgKeyStereoMode )) {
                m_StereoMode = !m_StereoMode;
                this.SetStereoMode( m_StereoMode ); // 不要
            }
        }

        /// <summary>GUI レイヤの描画？など</summary>
        public void OnGUI()
        {
            if( !m_AllowUpdate )
                return;

            if( m_StereoPower )
                GUI.Label( new Rect( 20,20, 200,50 ),
                        ( m_StereoMode ? "交差法" : "平行法" ) + "(" + m_cfgKeyStereoMode + "キーで切替)" );
            else
                GUI.Label( new Rect( 20,20, 200,50 ),
                        m_cfgKeyStereoPower + "キーで" + this.GetPluginName() + "をオン" );
        }

        /// <summary>立体視のオン/オフを設定する</summary>
        private bool SetStereoPower( bool power )
        {
            m_CameraL.gameObject.SetActive( power );
            m_CameraR.gameObject.SetActive( power );
            return( m_StereoPower = power );
        }

        /// <summary>立体視の交差法/並行法表示を設定する</summary>
        private bool SetStereoMode( bool mode )
        {
            return( m_StereoMode = mode );
        }

        /// <summary>設定ファイルから string データを読む</summary>
        private string GetPreferences( string section, string key, string defaultValue )
        {
            if (!Preferences.HasSection(section) || !Preferences[section].HasKey(key) || string.IsNullOrEmpty(Preferences[section][key].Value))
            {
                Preferences[section][key].Value = defaultValue;
                SaveConfig();
            }
            return Preferences[section][key].Value;
        }

        /// <summary>設定ファイルから float データを読む</summary>
        private float GetPreferences( string section, string key, float defaultValue )
        {
            if( !Preferences.HasSection( section ) || !Preferences[section].HasKey( key ) || string.IsNullOrEmpty( Preferences[section][key].Value ))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            float f = defaultValue;
            float.TryParse( Preferences[section][key].Value, out f );
            return f;
        }

        /// <summary>プラグイン名を取得する</summary>
        private String GetPluginName()
        {
            String name = String.Empty;
            try {
                // 属性クラスからプラグイン名取得
                PluginNameAttribute att = Attribute.GetCustomAttribute( typeof( BinbolusVR ), typeof( PluginNameAttribute )) as PluginNameAttribute;
                if( att != null )
                    name = att.Name;
            }
            catch( Exception e ) {
                Console.WriteLine( "{0}::GetPluginName: Exception: {1}", this.GetPluginName(), e.Message );
            }
            return name;
        }
    }
}