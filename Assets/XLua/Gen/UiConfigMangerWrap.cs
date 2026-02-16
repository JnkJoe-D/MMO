#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class UiConfigMangerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UiConfigManger);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 15, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetConfig", _m_GetConfig_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "AddConfig", _m_AddConfig_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RemoveConfig", _m_RemoveConfig_xlua_st_);
            
			
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "None", UiConfigManger.None);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Back", UiConfigManger.Back);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "System", UiConfigManger.System);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Menu", UiConfigManger.Menu);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Dialog", UiConfigManger.Dialog);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "MessageBox", UiConfigManger.MessageBox);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "MessageTip", UiConfigManger.MessageTip);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Story", UiConfigManger.Story);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Guide", UiConfigManger.Guide);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Loading", UiConfigManger.Loading);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Max", UiConfigManger.Max);
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new UiConfigManger();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UiConfigManger constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetConfig_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _viewId = LuaAPI.xlua_tointeger(L, 1);
                    
                        var gen_ret = UiConfigManger.GetConfig( _viewId );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddConfig_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UiConfig _uiConfig = (UiConfig)translator.GetObject(L, 1, typeof(UiConfig));
                    
                    UiConfigManger.AddConfig( _uiConfig );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RemoveConfig_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _viewId = LuaAPI.xlua_tointeger(L, 1);
                    
                    UiConfigManger.RemoveConfig( _viewId );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}
