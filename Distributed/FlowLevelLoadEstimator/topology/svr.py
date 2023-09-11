from sklearn.svm import SVR
import numpy as np
import pickle
from sklearn.model_selection import GridSearchCV

from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler

from sklearn.metrics import mean_squared_error
import matplotlib.pyplot as plt
import os


model_name='netsim/topology/svr_model.pkl'
model_svr_C=[0.1, 1, 10,100]
model_svr_gamma=[0.1, 1, 10]
model_svr_kernel=['linear', 'rbf', 'poly', 'sigmoid']

# 训练模型,确定最佳参数
def train_model(X,y):

  # 将数据集划分为训练集和测试集
  X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

  # 定义要调整的超参数
  param_grid = {'C': model_svr_C,
                  'gamma': model_svr_gamma}
                #   'kernel':model_svr_kernel}

  # 创建一个SVR模型
  svm_model = SVR()

  # 创建Grid Search对象
  grid_search = GridSearchCV(svm_model, param_grid=param_grid, cv=5, n_jobs=-1)

  # 运行Grid Search
  grid_search.fit(X_train, y_train)

  # 输出Grid Search的最佳参数
  print("Grid Search Best Params: ", grid_search.best_params_)
  print('Best accuracy: ', grid_search.best_score_)

  # 使用最佳参数来创建一个新的SVR模型
  # best_svm_model = SVR(C=grid_search.best_params_['C'], gamma=grid_search.best_params_['gamma'], kernel=grid_search.best_params_['kernel'])
  best_svm_model = SVR(C=grid_search.best_params_['C'], gamma=grid_search.best_params_['gamma'])

  # 使用训练数据来训练模型
  best_svm_model.fit(X_train, y_train)

  # 评估模型准确率
  score = best_svm_model.score(X_test, y_test)
  print(f"SVR模型的准确率为: {score:.2f}")

  # # 保存模型
  # save_model(best_svm_model,'svr_model.pkl')

  # 使用测试数据来评估模型性能
  # y_pred = best_svm_model.predict(X_test)
  # mse = mean_squared_error(y_test, y_pred)

  # print("MSE:", mse)


  # if is_show_images:
  #   # 绘制预测结果
  #   plt.scatter(X, y, color='black', label='Data')
  #   plt.plot(X, best_svm_model.predict(X), color='red', label='RBF model')
  #   plt.xlabel('X')
  #   plt.ylabel('y')
  #   plt.title('Support Vector Regression')
  #   plt.legend()
  #   plt.show()
  
  return best_svm_model

# 保存model
def save_model(model,name=model_name):
    # 保存模型和特征缩放器
    # with open('svr_model.pkl', 'wb') as f:
    #    pickle.dump({
    #     'model': model,
    #     'scaler': scaler,
    #     'C': model.C,
    #     'epsilon': model.epsilon
    # }, f)
    with open(name, 'wb') as f:
       pickle.dump(model, f)
    
# 读取model
def read_model(path=model_name):
    # 从文件中读取模型和特征缩放器
    # with open('svr_model.pkl', 'rb') as f:
    #   data = pickle.load(f)
    #   model = data['model']
    #   scaler = data['scaler']
    #   C = data['C']
    #   epsilon = data['epsilon']
    if os.path.exists(path):
        with open(path, 'rb') as f:
            with open(path, 'rb') as f:
                model = pickle.load(f)
                return model
    else:
        print("no model file！")
        return None
        
# 展示数据对比
def show_model_effect(X,Y,model):
    X1d=X[:, 0]
    # 绘制预测结果
    plt.scatter(X1d, Y, color='black', label='Data')
    plt.plot(X1d, model.predict(X), color='red', label='SVR model')
    plt.xlabel('X')
    plt.ylabel('y')
    plt.title('Support Vector Regression')
    plt.legend()
    plt.show()
