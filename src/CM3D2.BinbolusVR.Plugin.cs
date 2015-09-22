using System;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.BinbolusVR
{
    [PluginFilter( "CM3D2x64" ),
     PluginFilter( "CM3D2x86" ),
     PluginName( "BinbolusVR" ), PluginVersion( "0.0.1.6" )]

    public class BinbolusVR : PluginBase
    {
        #region メンバ変数定義：動作設定値
        // https://github.com/pirolix/CM3D2.BinbolusVR.Plugin/blob/master/README.md#設定ファイル
        private string          m_cfgSceneEnable        = "5,14,4,20";
        private string          m_cfgKeyStereoPower     = "K";
        private string          m_cfgKeyStereoMode      = "L";
        private float           m_cfgParallaxScale      = 0.1f;
        private string          m_cfgStereoPowers       = "NAKED_EYES";
        private string          m_cfgDefaultPower       = "OFF";
        private string          m_cfgDefaultMode        = "RL";
        // 視差スケール調整モード関係
        private const float     m_cfgParaSclAdjMax      = 1.0f;
        private const float     m_cfgParaSclAdjStep     = 0.01f;
        // http://docs.unity3d.com/Manual/ConventionalGameInput.html
        private const string    m_cfgParaSclAdjKeyInc   = "page up";
        private const string    m_cfgParaSclAdjKeyDec   = "page down";
        // デバッグモード関係
        private enum DEBUG_ENUMS {
            NONE                = 0,
            SHOW_CAPTION        = 1,
            PARALLAX_SCALE_ADJ  = 2,
        }
        private int             m_cfgDebug              = (int)DEBUG_ENUMS.SHOW_CAPTION;
        #endregion
        #region メンバ変数定義：状態管理関係
        private bool            m_bOculusVR             = false;
        private bool            m_AllowUpdate           = false;
        private enum STEREO_POWER_ENUMS {
        _ENUM_FIRST_VALUE = 0,
            OFF = _ENUM_FIRST_VALUE,
            NAKED_EYES,
            SIDEBYSIDE,
            TOPANDBOTTOM,
        _ENUM_MAX_VALUE,
        }
        private STEREO_POWER_ENUMS
                                m_StereoPower;
        private string          m_StereoMode;
        #endregion
        #region メンバ変数定義：オブジェクト
        private Camera          m_CameraL;
        private Camera          m_CameraR;
        #endregion

        /// <summary>プラグインが初期化されたタイミングで呼ばれるコンストラクタ</summary>
        public void Awake()
        {
            // VRモードでは動作しない
            m_bOculusVR = Application.dataPath.Contains( "CM3D2VRx64" );
            if( m_bOculusVR ) {
                Console.WriteLine( "{0}: Occuls Rift is not Support.", GetPluginName());
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

            // 現在の level が SceneEnable リストに含まれていたら有効にする
            if(( "," + m_cfgSceneEnable + "," ).Contains( level.ToString())
                    || m_cfgSceneEnable.ToUpper().Contains( "ALL" ))
            {
                // 左目用カメラ
                m_CameraL = (new GameObject( "ParallaxCameraL" )).AddComponent<Camera>();
                m_CameraL.CopyFrom( Camera.main );
                // 右目用カメラ
                m_CameraR = (new GameObject( "ParallaxCameraR" )).AddComponent<Camera>();
                m_CameraR.CopyFrom( Camera.main );

                SetStereoPower( m_cfgDefaultPower );
                SetStereoMode( m_cfgDefaultMode );

                m_AllowUpdate = true;
            }
        }

        /// <summary>画面を更新する</summary>
        public void LateUpdate()
        {
            if( !m_AllowUpdate )
                return;

            // 視差ベクトル
            Vector2 v = GameMain.Instance.MainCamera.GetAroundAngle();
            //Console.WriteLine( "AroundAngle x={0} Y={1}", v.x, v.y );
            Vector3 parallax = (new Vector3( Mathf.Cos( v.x * Mathf.Deg2Rad ), 0.0f, -Mathf.Sin( v.x * Mathf.Deg2Rad )))
                    * m_cfgParallaxScale
                    * GameMain.Instance.MainCamera.GetDistance()
                    * ( m_StereoMode == "RL" ? -1 : 1 );
                     
            if( STEREO_POWER_ENUMS.OFF != m_StereoPower ) {
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
            if( Input.GetKeyDown( m_cfgKeyStereoPower.ToLower())) {
                m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS.NAKED_EYES == m_StereoPower && !m_cfgStereoPowers.ToUpper().Contains( "NAKED_EYES" ))
                    m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS.SIDEBYSIDE == m_StereoPower && !m_cfgStereoPowers.ToUpper().Contains( "SIDEBYSIDE" ))
                    m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS.TOPANDBOTTOM == m_StereoPower && !m_cfgStereoPowers.ToUpper().Contains( "TOPANDBOTTOM" ))
                    m_StereoPower = m_StereoPower + 1;
                if( STEREO_POWER_ENUMS._ENUM_MAX_VALUE == m_StereoPower )
                    m_StereoPower = STEREO_POWER_ENUMS._ENUM_FIRST_VALUE;
                SetStereoPower( m_StereoPower );
            }

            // キー入力で切替える：平行法/交差法
            if( Input.GetKeyDown( m_cfgKeyStereoMode.ToLower())) {
                m_StereoMode = ( m_StereoMode == "RL" ? "LR" : "RL" );
                SetStereoMode( m_StereoMode ); // 不要
            }

            // ParallaxScale の調整モード
            if( (int)DEBUG_ENUMS.PARALLAX_SCALE_ADJ == m_cfgDebug )
            {
                if( Input.GetKeyDown( m_cfgParaSclAdjKeyInc ) && m_cfgParallaxScale < m_cfgParaSclAdjMax ) {
                    m_cfgParallaxScale += m_cfgParaSclAdjStep;
                    if( m_cfgParaSclAdjMax < m_cfgParallaxScale )
                        m_cfgParallaxScale = m_cfgParaSclAdjMax;
                }
                if( Input.GetKeyDown( m_cfgParaSclAdjKeyDec ) && 0.0f < m_cfgParallaxScale ) {
                    m_cfgParallaxScale -= m_cfgParaSclAdjStep;
                    if( m_cfgParallaxScale < 0.0f )
                        m_cfgParallaxScale = 0.0f;
                }
            }
        }

        /// <summary>GUI レイヤの描画？など</summary>
        public void OnGUI()
        {
            if( !m_AllowUpdate )
                return;

            if( (int)DEBUG_ENUMS.NONE != m_cfgDebug ) {
                string label_text = "";
                if( STEREO_POWER_ENUMS.OFF == m_StereoPower )
                    label_text = m_cfgKeyStereoPower + "キーで" + GetPluginName() + "をオン";
                else {
                    label_text = m_StereoPower.ToString() + "\n" +
                            ( m_StereoMode == "RL" ? "交差法" : "平行法" ) +
                            " (" + m_cfgKeyStereoMode + "キーで切替)\n";
                    if( (int)DEBUG_ENUMS.PARALLAX_SCALE_ADJ == m_cfgDebug )
                        label_text += "ParallaxScale=" + m_cfgParallaxScale.ToString("f2");
                }
                GUI.Label( new Rect( 20,20, 200,100 ), label_text );
            }
        }

        /// <summary>立体視のオン/オフを設定する</summary>
        private STEREO_POWER_ENUMS SetStereoPower( STEREO_POWER_ENUMS power )
        {
            // パワー ON/OFF
            m_CameraL.gameObject.SetActive( STEREO_POWER_ENUMS.OFF != power );
            m_CameraR.gameObject.SetActive( STEREO_POWER_ENUMS.OFF != power );
            // STEREO_POWER_ENUMS に応じて画面分割などを変化する
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

            case STEREO_POWER_ENUMS.OFF:
            default:
                m_CameraL.gameObject.SetActive( false );
                m_CameraR.gameObject.SetActive( false );
                break;
            }
            return( m_StereoPower = power );
        }

        /// <summary>立体視のオン/オフを設定する</summary>
        private STEREO_POWER_ENUMS SetStereoPower( string power )
        {
            switch( power.ToUpper()) {
            case "NAKED_EYES":
                return SetStereoPower( STEREO_POWER_ENUMS.NAKED_EYES );
            case "SIDEBYSIDE":
                return SetStereoPower( STEREO_POWER_ENUMS.SIDEBYSIDE );
            case "TOPANDBOTTOM":
                return SetStereoPower( STEREO_POWER_ENUMS.TOPANDBOTTOM );
            case "OFF":
            default:
                break;
            }
            return SetStereoPower( STEREO_POWER_ENUMS.OFF );
        }

        /// <summary>立体視の交差法/並行法表示を設定する</summary>
        private string SetStereoMode( string mode )
        {
            return( m_StereoMode = mode );
        }

        #region .ini ファイルの読み込み関係
        /// <summary>.ini ファイルからプラグイン設定を読み込む</summary>
        private void GetPluginPreferences()
        {
            m_cfgSceneEnable    = GetPreferences( "Config", "SceneEnable", m_cfgSceneEnable ).ToUpper();
            m_cfgKeyStereoPower = GetPreferences( "Config", "TogglePower", m_cfgKeyStereoPower ).ToUpper();
            m_cfgKeyStereoMode  = GetPreferences( "Config", "ToggleMode", m_cfgKeyStereoMode ).ToUpper();
            m_cfgParallaxScale  = GetPreferences( "Config", "ParallaxScale", m_cfgParallaxScale );
            m_cfgStereoPowers   = GetPreferences( "Config", "Powers", m_cfgStereoPowers ).ToUpper();
            m_cfgDefaultPower   = GetPreferences( "Config", "DefaultPower", m_cfgDefaultPower ).ToUpper();
            m_cfgDefaultMode    = GetPreferences( "Config", "DefaultMode", m_cfgDefaultMode ).ToUpper();
            m_cfgDebug          = GetPreferences( "Config", "DebugMode", m_cfgDebug );

            if( (int)DEBUG_ENUMS.NONE != m_cfgDebug ) {
                Console.WriteLine( "{0}: Config: SceneEnable= {1}", GetPluginName(), m_cfgSceneEnable );
                Console.WriteLine( "{0}: Config: ToggleKeyPower= {1}", GetPluginName(), m_cfgKeyStereoPower );
                Console.WriteLine( "{0}: Config: ToggleKeyMode= {1}", GetPluginName(), m_cfgKeyStereoMode );
                Console.WriteLine( "{0}: Config: ParallaxScale= {1}", GetPluginName(), m_cfgParallaxScale );
                Console.WriteLine( "{0}: Config: Powers= {1}", GetPluginName(), m_cfgStereoPowers );
                Console.WriteLine( "{0}: Config: DefaultPower= {1}", GetPluginName(), m_cfgDefaultPower );
                Console.WriteLine( "{0}: Config: DefaultMode= {1}", GetPluginName(), m_cfgDefaultMode );
                Console.WriteLine( "{0}: Config: DebugMode= {1}", GetPluginName(), m_cfgDebug );
            }
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

        /// <summary>設定ファイルから bool データを読む</summary>
        private bool GetPreferences( string section, string key, bool defaultValue )
        {
            if( !Preferences.HasSection( section ) || !Preferences[section].HasKey( key ) || string.IsNullOrEmpty( Preferences[section][key].Value ))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            bool b = defaultValue;
            bool.TryParse( Preferences[section][key].Value, out b );
            return b;
        }

        /// <summary>設定ファイルから int データを読む</summary>
        private int GetPreferences( string section, string key, int defaultValue )
        {
            if( !Preferences.HasSection( section ) || !Preferences[section].HasKey( key ) || string.IsNullOrEmpty( Preferences[section][key].Value ))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            int i = defaultValue;
            int.TryParse( Preferences[section][key].Value, out i );
            return i;
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
        #endregion

        #region 汎用メソッド
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
                Console.WriteLine( "{0}::GetPluginName: Exception: {1}", GetPluginName(), e.Message );
            }
            return name;
        }
        #endregion
    }
}