using System;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.BinbolusVR
{
    [PluginFilter( "CM3D2x64" ),
     PluginFilter( "CM3D2x86" ),
     PluginName( "BinbolusVR" ), PluginVersion( "0.0.1.5" )]

    public class BinbolusVR : PluginBase
    {
        private bool        m_bOculusVR = false;
        private bool        m_AllowUpdate = false;

        private Camera      m_CameraL;
        private Camera      m_CameraR;
        private enum STEREO_POWER_ENUMS {
            _ENUM_FIRST_VALUE = 0,
            POWER_OFF = _ENUM_FIRST_VALUE,
            NAKED_EYES,
            SIDEBYSIDE,
            TOPANDBOTTOM,
            _ENUM_MAX_VALUE,
        }
        private STEREO_POWER_ENUMS
                            m_StereoPower;
        private bool        m_StereoMode;

        private string      m_cfgKeyStereoPower = "k";
        private string      m_cfgKeyStereoMode  = "l";
        private float       m_cfgParallaxScale  = 0.1f;
        private string      m_cfgStereoPowers   = "NAKED_EYES";

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
            // 動作するシーン
            if( level == 5  // エディット
             || level == 14 // 夜伽
             || level == 4  // ダンス/ドキドキ☆Fallin' Love
             || level == 20 // ダンス/entracne to you
            ){
                // 左目用カメラ
                m_CameraL = (new GameObject( "ParallaxCameraL" )).AddComponent<Camera>();
                m_CameraL.CopyFrom( Camera.main );
                // 右目用カメラ
                m_CameraR = (new GameObject( "ParallaxCameraR" )).AddComponent<Camera>();
                m_CameraR.CopyFrom( Camera.main );

                SetStereoPower( STEREO_POWER_ENUMS.POWER_OFF );
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
            m_cfgStereoPowers = GetPreferences( "Config", "Powers", m_cfgStereoPowers ).ToLower();
            Console.WriteLine( "{0}: Config::Powers = {1}", this.GetPluginName(), m_cfgStereoPowers );
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
                    * GameMain.Instance.MainCamera.GetDistance()
                    * ( m_StereoMode ? -1 : 1 );
                     
            if( STEREO_POWER_ENUMS.POWER_OFF != m_StereoPower ) {
                // MainCamera が狙っている target:Vector3
                Vector3 target = GameMain.Instance.MainCamera.GetTargetPos();
                // カメラの場所を視差分だけずらして
                Transform mainCameraT = Camera.main.transform;
                m_CameraL.transform.position = mainCameraT.position - parallax;
                m_CameraR.transform.position = mainCameraT.position + parallax;
                // target を狙いなおす
                m_CameraL.transform.LookAt( target, mainCameraT.up );
                m_CameraR.transform.LookAt( target, mainCameraT.up );
            }
 
            // キー入力で切替える：オン/オフ
            if( Input.GetKeyDown( m_cfgKeyStereoPower )) {
                m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS.NAKED_EYES == m_StereoPower && !m_cfgStereoPowers.Contains( "NAKED_EYES".ToLower()))
                    m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS.SIDEBYSIDE == m_StereoPower && !m_cfgStereoPowers.Contains( "SIDEBYSIDE".ToLower()))
                    m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS.TOPANDBOTTOM == m_StereoPower && !m_cfgStereoPowers.Contains( "TOPANDBOTTOM".ToLower()))
                    m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS._ENUM_MAX_VALUE == m_StereoPower )
                    m_StereoPower = STEREO_POWER_ENUMS._ENUM_FIRST_VALUE;
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

            if( STEREO_POWER_ENUMS.POWER_OFF != m_StereoPower )
                GUI.Label( new Rect( 20,20, 200,50 ),
                        ( m_StereoMode ? "交差法" : "平行法" ) + "(" + m_cfgKeyStereoMode + "キーで切替)" );
            else
                GUI.Label( new Rect( 20,20, 200,50 ),
                        m_cfgKeyStereoPower + "キーで" + this.GetPluginName() + "をオン" );
        }

        /// <summary>立体視のオン/オフを設定する</summary>
        private STEREO_POWER_ENUMS SetStereoPower( STEREO_POWER_ENUMS power )
        {
            // パワー ON/OFF
            m_CameraL.gameObject.SetActive( STEREO_POWER_ENUMS.POWER_OFF != power );
            m_CameraR.gameObject.SetActive( STEREO_POWER_ENUMS.POWER_OFF != power );
            // パワーによって画面の分割などを変化する
            switch( power ) {
            case STEREO_POWER_ENUMS.NAKED_EYES:
                // 裸眼による交差法/平衡法
                m_CameraL.rect = new Rect( 0.0f, 0.0f, 0.5f, 1.0f );
                m_CameraR.rect = new Rect( 0.5f, 0.0f, 0.5f, 1.0f );
                m_CameraL.aspect = m_CameraR.aspect = 1.0f;
                break;

            case STEREO_POWER_ENUMS.SIDEBYSIDE:
                // HMD などの左右分割方式
                m_CameraL.rect = new Rect( 0.0f, 0.0f, 0.5f, 1.0f );
                m_CameraR.rect = new Rect( 0.5f, 0.0f, 0.5f, 1.0f );
                m_CameraL.aspect = m_CameraR.aspect = 2.0f;
                break;

            case STEREO_POWER_ENUMS.TOPANDBOTTOM:
                // HMD などの上下分割方式
                m_CameraL.rect = new Rect( 0.0f, 0.0f, 1.0f, 0.5f );
                m_CameraR.rect = new Rect( 0.0f, 0.5f, 1.0f, 0.5f );
                m_CameraL.aspect = m_CameraR.aspect = 2.0f;
                break;

            case STEREO_POWER_ENUMS.POWER_OFF:
            default:
                m_CameraL.gameObject.SetActive( false );
                m_CameraR.gameObject.SetActive( false );
                break;
            }
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