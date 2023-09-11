from enum import Enum


class ErrorCode(str, Enum):
    """
    错误码的枚举。对应ret_code，为0表示正常，负数为异常。
    "HTTP_"开头的表示http的status code.
    """
    OK = 0

    HTTP_OK = 200
    HTTP_BAD_REQUEST = 400
    HTTP_UNAUTHORIZED = 401
    HTTP_FORBIDDEN = 403
    HTTP_NOT_FOUND = 404
    HTTP_INTERNAL_SERVER_ERROR = 500

    E_SERVER = -500  # 服务器错误

    # 通用错误码
    E_PARAM_ERROR = -10
