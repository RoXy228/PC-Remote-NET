package com.pc.pcremotenet.network

import com.google.gson.Gson
import com.pc.pcremotenet.security.Crypto
import com.pc.pcremotenet.security.generateNonce
import java.io.DataInputStream
import java.io.DataOutputStream
import java.net.Socket
import java.nio.ByteBuffer
import java.nio.ByteOrder
import java.util.Base64

object NetworkClient {

    fun send(
        ip: String,
        port: Int,
        keyBase64: String,
        command: String
    ): RemoteResponse? {

        return try {

            val key = Base64.getDecoder().decode(keyBase64)

            val request = RemoteRequest(
                Timestamp = System.currentTimeMillis() / 1000,
                Nonce = generateNonce(),
                Command = command,
                Data = null
            )

            val json = Gson().toJson(request)

            val encrypted = Crypto.encrypt(
                key,
                json.toByteArray(Charsets.UTF_8)
            )

            val socket = Socket(ip, port)
            socket.soTimeout = 5000

            val output = DataOutputStream(socket.getOutputStream())
            val input = DataInputStream(socket.getInputStream())

            val lengthBytes = ByteBuffer
                .allocate(4)
                .order(ByteOrder.LITTLE_ENDIAN)
                .putInt(encrypted.size)
                .array()

            output.write(lengthBytes)
            output.write(encrypted)
            output.flush()

            val lenBuf = ByteArray(4)
            input.readFully(lenBuf)

            val len = ByteBuffer
                .wrap(lenBuf)
                .order(ByteOrder.LITTLE_ENDIAN)
                .int

            val data = ByteArray(len)
            input.readFully(data)

            socket.close()

            val decrypted = Crypto.decrypt(key, data)

            Gson().fromJson(
                String(decrypted),
                RemoteResponse::class.java
            )

        } catch (_: Exception) {
            null
        }
    }
}