package com.pc.pcremotenet.security

import android.util.Base64
import java.security.SecureRandom
import javax.crypto.Cipher
import javax.crypto.spec.GCMParameterSpec
import javax.crypto.spec.SecretKeySpec

object Crypto {

    private const val NONCE_SIZE = 12
    private const val TAG_SIZE = 16

    fun encrypt(key: ByteArray, plain: ByteArray): ByteArray {

        val nonce = ByteArray(NONCE_SIZE)
        SecureRandom().nextBytes(nonce)

        val cipher = Cipher.getInstance("AES/GCM/NoPadding")

        val keySpec = SecretKeySpec(key, "AES")
        val spec = GCMParameterSpec(TAG_SIZE * 8, nonce)

        cipher.init(Cipher.ENCRYPT_MODE, keySpec, spec)

        val encrypted = cipher.doFinal(plain)

        val result = ByteArray(nonce.size + encrypted.size)

        System.arraycopy(nonce,0,result,0,nonce.size)
        System.arraycopy(encrypted,0,result,nonce.size,encrypted.size)

        return result
    }

    fun base64(data: ByteArray): String {
        return Base64.encodeToString(data, Base64.NO_WRAP)
    }
    fun decrypt(key: ByteArray, data: ByteArray): ByteArray {

        val nonce = data.copyOfRange(0, NONCE_SIZE)
        val cipherText = data.copyOfRange(NONCE_SIZE, data.size)

        val cipher = Cipher.getInstance("AES/GCM/NoPadding")

        val keySpec = SecretKeySpec(key, "AES")
        val spec = GCMParameterSpec(TAG_SIZE * 8, nonce)

        cipher.init(Cipher.DECRYPT_MODE, keySpec, spec)

        return cipher.doFinal(cipherText)
    }
}

