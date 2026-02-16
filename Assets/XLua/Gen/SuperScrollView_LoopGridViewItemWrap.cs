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
    public class SuperScrollViewLoopGridViewItemWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(SuperScrollView.LoopGridViewItem);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 16, 15);
			
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserObjectData", _g_get_UserObjectData);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserIntData1", _g_get_UserIntData1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserIntData2", _g_get_UserIntData2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserStringData1", _g_get_UserStringData1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserStringData2", _g_get_UserStringData2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemCreatedCheckFrameCount", _g_get_ItemCreatedCheckFrameCount);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "CachedRectTransform", _g_get_CachedRectTransform);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemPrefabName", _g_get_ItemPrefabName);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Row", _g_get_Row);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Column", _g_get_Column);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemIndex", _g_get_ItemIndex);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemId", _g_get_ItemId);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsInitHandlerCalled", _g_get_IsInitHandlerCalled);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ParentGridView", _g_get_ParentGridView);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "PrevItem", _g_get_PrevItem);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "NextItem", _g_get_NextItem);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserObjectData", _s_set_UserObjectData);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserIntData1", _s_set_UserIntData1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserIntData2", _s_set_UserIntData2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserStringData1", _s_set_UserStringData1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserStringData2", _s_set_UserStringData2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemCreatedCheckFrameCount", _s_set_ItemCreatedCheckFrameCount);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemPrefabName", _s_set_ItemPrefabName);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Row", _s_set_Row);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Column", _s_set_Column);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemIndex", _s_set_ItemIndex);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemId", _s_set_ItemId);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IsInitHandlerCalled", _s_set_IsInitHandlerCalled);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ParentGridView", _s_set_ParentGridView);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "PrevItem", _s_set_PrevItem);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "NextItem", _s_set_NextItem);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new SuperScrollView.LoopGridViewItem();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to SuperScrollView.LoopGridViewItem constructor!");
            
        }
        
		
        
		
        
        
        
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UserObjectData(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.UserObjectData);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UserIntData1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.UserIntData1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UserIntData2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.UserIntData2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UserStringData1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.UserStringData1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UserStringData2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.UserStringData2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemCreatedCheckFrameCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ItemCreatedCheckFrameCount);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_CachedRectTransform(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.CachedRectTransform);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemPrefabName(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.ItemPrefabName);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Row(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Row);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Column(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Column);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemIndex(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ItemIndex);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemId(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ItemId);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsInitHandlerCalled(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsInitHandlerCalled);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ParentGridView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ParentGridView);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_PrevItem(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.PrevItem);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_NextItem(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.NextItem);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UserObjectData(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UserObjectData = translator.GetObject(L, 2, typeof(object));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UserIntData1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UserIntData1 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UserIntData2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UserIntData2 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UserStringData1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UserStringData1 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UserStringData2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UserStringData2 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ItemCreatedCheckFrameCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemCreatedCheckFrameCount = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ItemPrefabName(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemPrefabName = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Row(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Row = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Column(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Column = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ItemIndex(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemIndex = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ItemId(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemId = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IsInitHandlerCalled(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IsInitHandlerCalled = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ParentGridView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ParentGridView = (SuperScrollView.LoopGridView)translator.GetObject(L, 2, typeof(SuperScrollView.LoopGridView));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_PrevItem(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.PrevItem = (SuperScrollView.LoopGridViewItem)translator.GetObject(L, 2, typeof(SuperScrollView.LoopGridViewItem));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_NextItem(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopGridViewItem gen_to_be_invoked = (SuperScrollView.LoopGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.NextItem = (SuperScrollView.LoopGridViewItem)translator.GetObject(L, 2, typeof(SuperScrollView.LoopGridViewItem));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
