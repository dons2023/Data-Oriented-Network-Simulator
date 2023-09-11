import datetime
import logging.config
import os

# Quick-start development settings - unsuitable for production
# See https://docs.djangoproject.com/en/2.0/howto/deployment/checklist/

# SECURITY WARNING: keep the secret key used in production secret!
SECRET_KEY = '123456'

LOGLEVEL = os.environ.get('LOGLEVEL', 'debug').upper()


def skip_static_requests(record):
    if record.args[0].startswith('GET /static/'):  # filter whatever you want
        return False
    return True


logging.config.dictConfig({
    'version': 1,
    'disable_existing_loggers': False,
    'filters': {
        # use Django's built in CallbackFilter to point to your filter
        'skip_static_requests': {
            '()': 'django.utils.log.CallbackFilter',
            'callback': skip_static_requests
        }
    },
    'formatters': {
        'console': {
            'format': '[%(asctime)s] %(levelname)s %(module)s: %(message)s',
            'datefmt': '%Y-%m-%d %H:%M',
        }
    },
    'handlers': {
        'console': {
            'class': 'logging.StreamHandler',
            'formatter': 'console',
        },
        'django.server': {
            'level': 'INFO',
            'filters': ['skip_static_requests'],  # <- ...with one change
            'class': 'logging.StreamHandler',
        },
    },
    'loggers': {
        'topology': {
            'level': LOGLEVEL,
            'handlers': ['console'],
            'propagate': False,
        },
    },
})

ALLOWED_HOSTS = ['*']


JWT_AUTH = {
    'JWT_EXPIRATION_DELTA': datetime.timedelta(days=1),
}

REST_FRAMEWORK = {
    'DEFAULT_AUTHENTICATION_CLASSES': (
        'rest_framework_jwt.authentication.JSONWebTokenAuthentication',
        'rest_framework.authentication.SessionAuthentication',
    )
}

DEFAULT_PERMISSION_CLASSES = {
    'rest_framework.permissions.IsAuthenticated'
}

# Application definition
INSTALLED_APPS = [
    'topology.equipments',
    ''
    # 'topology.users',
    'rest_framework',
    'webpack_loader',
    'django.contrib.admin',
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'django.contrib.staticfiles',
    'django_extensions',
]

# AUTH_USER_MODEL = 'topology.user.User'

MIDDLEWARE = [
    'django.middleware.security.SecurityMiddleware',
    'django.contrib.sessions.middleware.SessionMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
]

ROOT_URLCONF = 'topology.urls'

TEMPLATES = [
    {
        'BACKEND': 'django.template.backends.django.DjangoTemplates',
        'DIRS': [],
        'APP_DIRS': True,
        'OPTIONS': {
            'context_processors': [
                'django.template.context_processors.debug',
                'django.template.context_processors.request',
                'django.contrib.auth.context_processors.auth',
                'django.contrib.messages.context_processors.messages',
            ],
        },
    },
]

WSGI_APPLICATION = 'topology.wsgi.application'

# Password validation
# https://docs.djangoproject.com/en/2.0/ref/settings/#auth-password-validators

AUTH_PASSWORD_VALIDATORS = [
    {
        'NAME': 'django.contrib.auth.password_validation.UserAttributeSimilarityValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.MinimumLengthValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.CommonPasswordValidator',
    },
    {
        'NAME': 'django.contrib.auth.password_validation.NumericPasswordValidator',
    },
]

# Internationalization
# https://docs.djangoproject.com/en/3.1/topics/i18n/

LANGUAGE_CODE = 'en-us'

USE_TZ = False

TIME_ZONE = 'Asia/Shanghai'

USE_I18N = True

USE_L10N = True

# Static files (CSS, JavaScript, Images)
# https://docs.djangoproject.com/en/3.1/howto/static-files/

STATIC_URL = '/public/'
