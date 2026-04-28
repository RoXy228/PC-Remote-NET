package com.pc.pcremotenet.data

import android.content.Context
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow

class AppPrefs(context: Context) {

    private val prefs =
        context.getSharedPreferences("pcremote", Context.MODE_PRIVATE)

    private val _externalIp =
        MutableStateFlow(prefs.getString("external_ip", "") ?: "")

    val externalIp: StateFlow<String> = _externalIp

    private val _aesKey =
        MutableStateFlow(prefs.getString("aes_key", "") ?: "")

    val aesKey: StateFlow<String> = _aesKey

    private val _wolEnabled =
        MutableStateFlow(prefs.getBoolean("wol_enabled", false))

    val wolEnabled: StateFlow<Boolean> = _wolEnabled

    var port: Int
        get() = prefs.getInt("port", 5055)
        set(value) = prefs.edit().putInt("port", value).apply()

    val wolPort = 40000

    fun setExternalIp(value: String) {
        prefs.edit().putString("external_ip", value).apply()
        _externalIp.value = value
    }

    fun setAesKey(value: String) {
        prefs.edit().putString("aes_key", value).apply()
        _aesKey.value = value
    }

    fun setWolEnabled(value: Boolean) {
        prefs.edit().putBoolean("wol_enabled", value).apply()
        _wolEnabled.value = value
    }
    private val _pcName =
        MutableStateFlow(prefs.getString("pc_name", "Мой ПК") ?: "Мой ПК")

    val pcName: StateFlow<String> = _pcName

    fun setPcName(value: String) {
        prefs.edit().putString("pc_name", value).apply()
        _pcName.value = value
    }

    var pcMac: String
        get() = prefs.getString("pc_mac", "") ?: ""
        set(value) {
            prefs.edit().putString("pc_mac", value).apply()
        }
    private val _localIp =
        MutableStateFlow(prefs.getString("local_ip", "") ?: "")

    val localIp: StateFlow<String> = _localIp

    fun setLocalIp(value: String) {
        prefs.edit().putString("local_ip", value).apply()
        _localIp.value = value
    }
    private val _useLocalIp =
        MutableStateFlow(prefs.getBoolean("use_local_ip", false))

    val useLocalIp: StateFlow<Boolean> = _useLocalIp

    fun setUseLocalIp(value: Boolean) {
        prefs.edit().putBoolean("use_local_ip", value).apply()
        _useLocalIp.value = value
    }
}