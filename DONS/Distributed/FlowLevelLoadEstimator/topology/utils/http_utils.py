from rest_framework.response import Response

from constants.error_code import ErrorCode


def response_data(data=None, code=ErrorCode.HTTP_OK, message=''):
    if data is None:
        data = {}
    return Response({'data': data, 'code': code, 'errMsg': message})
