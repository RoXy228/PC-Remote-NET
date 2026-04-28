package com.pc.pcremotenet.network

import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress

fun sendWakeOnLan(
    mac: String,
    host: String,
    port: Int = 40000,
    broadcastMode: Boolean = true
) {

    Thread {

        try {

            val cleanMac = mac.replace(":", "").replace("-", "")

            val macBytes = ByteArray(6)

            for (i in 0 until 6) {
                macBytes[i] =
                    cleanMac.substring(i * 2, i * 2 + 2)
                        .toInt(16)
                        .toByte()
            }

            val packet = ByteArray(6 + 16 * macBytes.size)

            for (i in 0 until 6)
                packet[i] = 0xFF.toByte()

            for (i in 6 until packet.size step macBytes.size)
                System.arraycopy(macBytes, 0, packet, i, macBytes.size)

            val socket = DatagramSocket()

            // КРИТИЧНО
            socket.broadcast = true

            if (broadcastMode) {

                val subnetBroadcast = calculateBroadcast(host)

                val addresses = listOf(
                    "255.255.255.255",
                    subnetBroadcast
                )

                for (addr in addresses) {

                    try {

                        val address = InetAddress.getByName(addr)

                        val datagram =
                            DatagramPacket(packet, packet.size, address, port)

                        socket.send(datagram)

                    } catch (_: Exception) {}
                }

            } else {

                val address = InetAddress.getByName(host)

                val datagram =
                    DatagramPacket(packet, packet.size, address, port)

                socket.send(datagram)

            }

            socket.close()

        } catch (_: Exception) {}

    }.start()
}
private fun calculateBroadcast(ip: String): String {
    return try {
        val parts = ip.split(".").map { it.toInt() }.toMutableList()
        if (parts.size == 4) {
            parts[3] = 255
            parts.joinToString(".")
        } else {
            "255.255.255.255"
        }
    } catch (_: Exception) {
        "255.255.255.255"
    }
}