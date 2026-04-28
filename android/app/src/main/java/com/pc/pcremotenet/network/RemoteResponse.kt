package com.pc.pcremotenet.network

data class RemoteResponse(
    val Ok: Boolean,
    val Message: String? = null,
    val ErrorCode: Int? = null
)