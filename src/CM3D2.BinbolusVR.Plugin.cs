using System;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
namespace CM3D2.BinbolusVR
{
    [PluginName( "BinbolusVRforC(O)M3D2" ), PluginVersion( "1.8.0.0" )]

    public class BinbolusVR : PluginBase
    {
        #region メンバ変数定義：動作設定値
        private enum DEBUG_LEVELS {
            NONE                = 0,
            SHOW_CAPTION        = 1,
            PARALLAX_SCALE_ADJ  = 2,
        }
        private struct _CONFIG {
            // https://github.com/pirolix/CM3D2.BinbolusVR.Plugin/blob/master/README.md#設定ファイル
            public string       ScenesEnable;
            public string       PowersEnable;
            public string       KeyTogglePower;
            public string       KeyToggleMode;
            public float        ParallaxScale;
            public string       DefaultPower;
            public string       DefaultMode;
            public int          DebugLevel; // @see DEBUG_LEVELS enum

            // 視差スケール調整モード関係
            public const float  ParaSclAdjMax       = 1.0f;
            public const float  ParaSclAdjStep      = 0.001f;
            public string       ParaSclAdjKeyInc;
            public string       ParaSclAdjKeyDec;
        }
        private _CONFIG         m_cfg;
        #endregion
        #region メンバ変数定義：状態管理関係
        private bool            m_bOculusVRCM             = false;
        private bool            m_bOculusVRCOM            = false;
        private bool            m_AllowUpdate           = false;
        private enum POWERS {
        _ENUM_FIRST_VALUE = 0,
            OFF = _ENUM_FIRST_VALUE,
            NAKED_EYES,
            SIDEBYSIDE,
            TOPANDBOTTOM,
        _ENUM_MAX_VALUE,
        }
        private POWERS          m_Power;
        private string          m_Mode;
        #endregion
        #region メンバ変数定義：オブジェクト
        private Camera          m_CameraL;
        private Camera          m_CameraR;
        #endregion

        /// <summary>プラグインが初期化されたタイミングで呼ばれるコンストラクタ</summary>
        public void Awake()
        {
            // 動作設定値の初期値
            m_cfg.DebugLevel        = (int)DEBUG_LEVELS.SHOW_CAPTION;
            m_cfg.ScenesEnable      = "5,14,4,20";
            m_cfg.PowersEnable      = "NAKED_EYES";
            m_cfg.ParallaxScale     = 0.1f;
            m_cfg.DefaultPower      = "OFF";
            m_cfg.DefaultMode       = "RL";
            m_cfg.KeyTogglePower    = "K";
            m_cfg.KeyToggleMode     = "L";
            m_cfg.ParaSclAdjKeyInc  = "Page Up";
            m_cfg.ParaSclAdjKeyDec  = "Page Down";

            // VRモードでは動作しない
            m_bOculusVRCM = Application.dataPath.Contains( "CM3D2VRx64" );
            m_bOculusVRCOM = Application.dataPath.Contains( "COM3D2VRx64" );
            if( m_bOculusVRCM || m_bOculusVRCOM) {
                Console.WriteLine( "{0}:you are playing with True VR, so binbolusVR is off", GetPluginName());
                return;
            }
            GameObject.DontDestroyOnLoad( this );
            GetPluginPreferences();
        }

        /// <summary>ゲームレベルが変化した際に呼ばれる</summary>
        public void OnLevelWasLoaded(int level)
        {
            m_AllowUpdate = false;
            if( m_bOculusVRCM || m_bOculusVRCOM )
                return;
            //Console.WriteLine( "now scene is {0}",SceneManager.GetActiveScene().name);
            bool isdance = false;
            //scene名を正規表現で検出し ダンスの場合isdanceをtrueにセット
            isdance = Regex.IsMatch(SceneManager.GetActiveScene().name, "SceneDance_.+_Release");
            //Console.WriteLine( "now isdance is {0}",isdance);
            // 現在の level が ScenesEnable リストに含まれている or ダンスシーンなら有効にする
            if(( "," + m_cfg.ScenesEnable + "," ).Contains( level.ToString())
                    || m_cfg.ScenesEnable.ToUpper().Contains( "ALL" ) || isdance)
            {
                //Console.WriteLine( "now level is {0},binbolus plugin is called",level);
                // 左目用カメラ
                m_CameraL = (new GameObject( "ParallaxCameraL" )).AddComponent<Camera>();
                m_CameraL.CopyFrom( Camera.main );
                // 右目用カメラ
                m_CameraR = (new GameObject( "ParallaxCameraR" )).AddComponent<Camera>();
                m_CameraR.CopyFrom( Camera.main );

                SetStereoPower( m_cfg.DefaultPower );
                SetStereoMode( m_cfg.DefaultMode );

                m_AllowUpdate = true;
            }
        }

        /// <summary>画面を更新する</summary>
        public void LateUpdate()
        {
            if( !m_AllowUpdate )
                return;

            if( POWERS.OFF != m_Power ) {
                Transform mainCameraT = Camera.main.transform;
                //メインカメラが向いている方向を保存
                Vector3 v = mainCameraT.transform.localEulerAngles;
                //メインカメラの向きに応じて視差を生成 
                Vector3 parallax = (new Vector3( Mathf.Cos( v.y * Mathf.Deg2Rad ) * Mathf.Cos(v.z * Mathf.Deg2Rad ),
                             Mathf.Cos(v.x * Mathf.Deg2Rad) * Mathf.Sin(v.z * Mathf.Deg2Rad), -Mathf.Sin( v.y * Mathf.Deg2Rad ) * Mathf.Cos(v.z * Mathf.Deg2Rad)))
                    * m_cfg.ParallaxScale
                    * ( m_Mode == "RL" ? -1 : 1 );
                //Console.WriteLine( "AroundAngle x={0} y={1},z={2}", v.x, v.y, v.z);
                // カメラの場所を視差分だけずらして
                m_CameraL.transform.position = mainCameraT.position - parallax;
                m_CameraR.transform.position = mainCameraT.position + parallax;
                // メインカメラの向き(保存済み)に合わせて両目カメラを向ける
                m_CameraL.transform.localEulerAngles = v;
                m_CameraR.transform.localEulerAngles = v;
            }

            // キー入力で切替える：オン/オフ
            if( Input.GetKeyDown( m_cfg.KeyTogglePower.ToLower())) {
                m_Power = m_Power + 1;
                if( POWERS.NAKED_EYES == m_Power && !m_cfg.PowersEnable.ToUpper().Contains( "NAKED_EYES" ))
                    m_Power = m_Power + 1;
                if( POWERS.SIDEBYSIDE == m_Power && !m_cfg.PowersEnable.ToUpper().Contains( "SIDEBYSIDE" ))
                    m_Power = m_Power + 1;
                if( POWERS.TOPANDBOTTOM == m_Power && !m_cfg.PowersEnable.ToUpper().Contains( "TOPANDBOTTOM" ))
                    m_Power = m_Power + 1;
                if( POWERS._ENUM_MAX_VALUE == m_Power )
                    m_Power = POWERS._ENUM_FIRST_VALUE;
                SetStereoPower( m_Power );
            }

            // キー入力で切替える：平行法/交差法
            if( Input.GetKeyDown( m_cfg.KeyToggleMode.ToLower())) {
                m_Mode = ( m_Mode == "RL" ? "LR" : "RL" );
                SetStereoMode( m_Mode ); // 不要
            }

            // ParallaxScale の調整モード
            if( (int)DEBUG_LEVELS.PARALLAX_SCALE_ADJ == m_cfg.DebugLevel )
            {
                if( Input.GetKey( m_cfg.ParaSclAdjKeyInc.ToLower()) && m_cfg.ParallaxScale < _CONFIG.ParaSclAdjMax ) {
                    m_cfg.ParallaxScale += _CONFIG.ParaSclAdjStep
                            * ( Input.GetKey( KeyCode.RightShift ) ? 10 : 1 );
                    if( _CONFIG.ParaSclAdjMax < m_cfg.ParallaxScale )
                        m_cfg.ParallaxScale = _CONFIG.ParaSclAdjMax;
                }
                if( Input.GetKey( m_cfg.ParaSclAdjKeyDec.ToLower()) && 0.0f < m_cfg.ParallaxScale ) {
                    m_cfg.ParallaxScale -= _CONFIG.ParaSclAdjStep
                            * ( Input.GetKey( KeyCode.RightShift ) ? 10 : 1 );
                    if( m_cfg.ParallaxScale < 0.0f )
                        m_cfg.ParallaxScale = 0.0f;
                }
            }
        }

        /// <summary>GUI レイヤの描画？など</summary>
        public void OnGUI()
        {
            if( !m_AllowUpdate )
                return;

            if( (int)DEBUG_LEVELS.NONE != m_cfg.DebugLevel ) {
                string label_text = "";
                if( POWERS.OFF == m_Power )
                    label_text = m_cfg.KeyTogglePower + "キーで" + GetPluginName() + "をオン";
                else {
                    label_text = m_Power.ToString() + "\n" +
                            ( m_Mode == "RL" ? "交差法" : "平行法" ) +
                            " (" + m_cfg.KeyToggleMode + "キーで切替)\n";
                    if( (int)DEBUG_LEVELS.PARALLAX_SCALE_ADJ == m_cfg.DebugLevel )
                        label_text += "ParallaxScale=" + m_cfg.ParallaxScale.ToString("f3");
                }
                GUI.Label( new Rect( 20,20, 200,100 ), label_text );
            }
        }

        /// <summary>立体視のオン/オフを設定する</summary>
        private POWERS SetStereoPower( POWERS power )
        {
            // パワー ON/OFF
            m_CameraL.gameObject.SetActive( POWERS.OFF != power );
            m_CameraR.gameObject.SetActive( POWERS.OFF != power );
            // POWERS に応じて画面分割などを変化する
            switch( power ) {
            case POWERS.NAKED_EYES:
                // 裸眼による交差法/平衡法
                m_CameraL.rect = new Rect( 0.0f, 0.0f, 0.5f, 1.0f );
                m_CameraR.rect = new Rect( 0.5f, 0.0f, 0.5f, 1.0f );
                m_CameraL.aspect = m_CameraR.aspect = 1.0f;
                break;

            case POWERS.SIDEBYSIDE:
                // HMD などの左右分割方式
                m_CameraL.rect = new Rect( 0.0f, 0.0f, 0.5f, 1.0f );
                m_CameraR.rect = new Rect( 0.5f, 0.0f, 0.5f, 1.0f );
                m_CameraL.aspect = m_CameraR.aspect = 2.0f;
                break;

            case POWERS.TOPANDBOTTOM:
                // HMD などの上下分割方式
                m_CameraL.rect = new Rect( 0.0f, 0.0f, 1.0f, 0.5f );
                m_CameraR.rect = new Rect( 0.0f, 0.5f, 1.0f, 0.5f );
                m_CameraL.aspect = m_CameraR.aspect = 2.0f;
                break;

            case POWERS.OFF:
            default:
                m_CameraL.gameObject.SetActive( false );
                m_CameraR.gameObject.SetActive( false );
                break;
            }
            return( m_Power = power );
        }

        /// <summary>立体視のオン/オフを設定する</summary>
        private POWERS SetStereoPower( string power )
        {
            switch( power.ToUpper()) {
            case "NAKED_EYES":
                return SetStereoPower( POWERS.NAKED_EYES );
            case "SIDEBYSIDE":
                return SetStereoPower( POWERS.SIDEBYSIDE );
            case "TOPANDBOTTOM":
                return SetStereoPower( POWERS.TOPANDBOTTOM );
            case "OFF":
            default:
                break;
            }
            return SetStereoPower( POWERS.OFF );
        }

        /// <summary>立体視の交差法/並行法表示を設定する</summary>
        private string SetStereoMode( string mode )
        {
            return( m_Mode = mode );
        }

        #region .ini ファイルの読み込み関係
        /// <summary>.ini ファイルからプラグイン設定を読み込む</summary>
        private void GetPluginPreferences()
        {
            m_cfg.DebugLevel     = GetPreferences( "Config", "DebugLevel", m_cfg.DebugLevel );
            m_cfg.ScenesEnable   = GetPreferences( "Config", "ScenesEnable", m_cfg.ScenesEnable ).ToUpper();
            m_cfg.PowersEnable   = GetPreferences( "Config", "PowersEnable", m_cfg.PowersEnable ).ToUpper();
            m_cfg.DefaultPower   = GetPreferences( "Config", "DefaultPower", m_cfg.DefaultPower ).ToUpper();
            m_cfg.DefaultMode    = GetPreferences( "Config", "DefaultMode", m_cfg.DefaultMode ).ToUpper();
            m_cfg.ParallaxScale  = GetPreferences( "Config", "ParallaxScale", m_cfg.ParallaxScale );
            m_cfg.KeyTogglePower = GetPreferences( "Key", "TogglePower", m_cfg.KeyTogglePower ).ToUpper();
            m_cfg.KeyToggleMode  = GetPreferences( "Key", "ToggleMode", m_cfg.KeyToggleMode ).ToUpper();
            m_cfg.ParaSclAdjKeyInc  = GetPreferences( "Key", "ParaSclAdjKeyInc", m_cfg.ParaSclAdjKeyInc ).ToUpper();
            m_cfg.ParaSclAdjKeyDec  = GetPreferences( "Key", "ParaSclAdjKeyDec", m_cfg.ParaSclAdjKeyDec ).ToUpper();

            if( (int)DEBUG_LEVELS.NONE != m_cfg.DebugLevel ) {
                Console.WriteLine( "{0}: Config: DebugLevel= {1}", GetPluginName(), m_cfg.DebugLevel );
                Console.WriteLine( "{0}: Config: ScenesEnable= {1}", GetPluginName(), m_cfg.ScenesEnable );
                Console.WriteLine( "{0}: Config: PowersEnable= {1}", GetPluginName(), m_cfg.PowersEnable );
                Console.WriteLine( "{0}: Config: DefaultPower= {1}", GetPluginName(), m_cfg.DefaultPower );
                Console.WriteLine( "{0}: Config: DefaultMode= {1}", GetPluginName(), m_cfg.DefaultMode );
                Console.WriteLine( "{0}: Config: ParallaxScale= {1}", GetPluginName(), m_cfg.ParallaxScale );
                // キー設定 @see http://docs.unity3d.com/Manual/ConventionalGameInput.html
                Console.WriteLine( "{0}: Key: KeyTogglePower= {1}", GetPluginName(), m_cfg.KeyTogglePower );
                Console.WriteLine( "{0}: Key: KeyToggleMode= {1}", GetPluginName(), m_cfg.KeyToggleMode );
                Console.WriteLine( "{0}: Key: ParaSclAdjKeyInc= {1}", GetPluginName(), m_cfg.ParaSclAdjKeyInc );
                Console.WriteLine( "{0}: Key: ParaSclAdjKeyDec= {1}", GetPluginName(), m_cfg.ParaSclAdjKeyDec );
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
