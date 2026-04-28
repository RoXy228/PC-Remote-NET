package com.pc.pcremotenet.network

data class RemoteRequest(
    val Version: Int = 1,
    val Timestamp: Long,
    val Nonce: String,
    val Command: String,
    val Data: String? = null
)