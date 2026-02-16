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
    public class SuperScrollViewLoopStaggeredGridViewItemWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(SuperScrollView.LoopStaggeredGridViewItem);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 23, 16);
			
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserObjectData", _g_get_UserObjectData);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserIntData1", _g_get_UserIntData1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserIntData2", _g_get_UserIntData2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserStringData1", _g_get_UserStringData1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UserStringData2", _g_get_UserStringData2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "DistanceWithViewPortSnapCenter", _g_get_DistanceWithViewPortSnapCenter);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "StartPosOffset", _g_get_StartPosOffset);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemCreatedCheckFrameCount", _g_get_ItemCreatedCheckFrameCount);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Padding", _g_get_Padding);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ExtraPadding", _g_get_ExtraPadding);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "CachedRectTransform", _g_get_CachedRectTransform);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemPrefabName", _g_get_ItemPrefabName);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemIndexInGroup", _g_get_ItemIndexInGroup);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemIndex", _g_get_ItemIndex);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemId", _g_get_ItemId);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsInitHandlerCalled", _g_get_IsInitHandlerCalled);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ParentListView", _g_get_ParentListView);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "TopY", _g_get_TopY);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "BottomY", _g_get_BottomY);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "LeftX", _g_get_LeftX);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "RightX", _g_get_RightX);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemSize", _g_get_ItemSize);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemSizeWithPadding", _g_get_ItemSizeWithPadding);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserObjectData", _s_set_UserObjectData);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserIntData1", _s_set_UserIntData1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserIntData2", _s_set_UserIntData2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserStringData1", _s_set_UserStringData1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UserStringData2", _s_set_UserStringData2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "DistanceWithViewPortSnapCenter", _s_set_DistanceWithViewPortSnapCenter);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "StartPosOffset", _s_set_StartPosOffset);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemCreatedCheckFrameCount", _s_set_ItemCreatedCheckFrameCount);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Padding", _s_set_Padding);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ExtraPadding", _s_set_ExtraPadding);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemPrefabName", _s_set_ItemPrefabName);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemIndexInGroup", _s_set_ItemIndexInGroup);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemIndex", _s_set_ItemIndex);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ItemId", _s_set_ItemId);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IsInitHandlerCalled", _s_set_IsInitHandlerCalled);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ParentListView", _s_set_ParentListView);
            
			
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
					
					var gen_ret = new SuperScrollView.LoopStaggeredGridViewItem();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to SuperScrollView.LoopStaggeredGridViewItem constructor!");
            
        }
        
		
        
		
        
        
        
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UserObjectData(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.UserStringData2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_DistanceWithViewPortSnapCenter(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.DistanceWithViewPortSnapCenter);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_StartPosOffset(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.StartPosOffset);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ItemCreatedCheckFrameCount);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Padding(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.Padding);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ExtraPadding(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.ExtraPadding);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.ItemPrefabName);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemIndexInGroup(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ItemIndexInGroup);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsInitHandlerCalled);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ParentListView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ParentListView);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_TopY(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.TopY);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_BottomY(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.BottomY);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_LeftX(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.LeftX);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_RightX(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.RightX);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemSize(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.ItemSize);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemSizeWithPadding(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.ItemSizeWithPadding);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UserStringData2 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_DistanceWithViewPortSnapCenter(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.DistanceWithViewPortSnapCenter = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_StartPosOffset(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.StartPosOffset = (float)LuaAPI.lua_tonumber(L, 2);
            
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemCreatedCheckFrameCount = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Padding(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Padding = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ExtraPadding(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ExtraPadding = (float)LuaAPI.lua_tonumber(L, 2);
            
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemPrefabName = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ItemIndexInGroup(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ItemIndexInGroup = LuaAPI.xlua_tointeger(L, 2);
            
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
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
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IsInitHandlerCalled = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ParentListView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridViewItem gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridViewItem)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ParentListView = (SuperScrollView.LoopStaggeredGridView)translator.GetObject(L, 2, typeof(SuperScrollView.LoopStaggeredGridView));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
