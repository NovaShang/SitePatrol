/** Generate by swagger-axios-codegen */
// @ts-nocheck
/* eslint-disable */

/** Generate by swagger-axios-codegen */
/* eslint-disable */
// @ts-nocheck
import axiosStatic, { AxiosInstance, AxiosRequestConfig } from 'axios';

export interface IRequestOptions extends AxiosRequestConfig {
  /**
   * show loading status
   */
  loading?: boolean;
  /**
   * display error message
   */
  showError?: boolean;
  /**
   * indicates whether Authorization credentials are required for the request
   * @default true
   */
  withAuthorization?: boolean;
}

export interface IRequestConfig {
  method?: any;
  headers?: any;
  url?: any;
  data?: any;
  params?: any;
}

// Add options interface
export interface ServiceOptions {
  axios?: AxiosInstance;
  /** only in axios interceptor config*/
  loading: boolean;
  showError: boolean;
}

// Add default options
export const serviceOptions: ServiceOptions = {};

// Instance selector
export function axios(configs: IRequestConfig, resolve: (p: any) => void, reject: (p: any) => void): Promise<any> {
  if (serviceOptions.axios) {
    return serviceOptions.axios
      .request(configs)
      .then((res) => {
        resolve(res.data);
      })
      .catch((err) => {
        reject(err);
      });
  } else {
    throw new Error('please inject yourself instance like axios  ');
  }
}

export function getConfigs(method: string, contentType: string, url: string, options: any): IRequestConfig {
  const configs: IRequestConfig = {
    loading: serviceOptions.loading,
    showError: serviceOptions.showError,
    ...options,
    method,
    url
  };
  configs.headers = {
    ...options.headers,
    'Content-Type': contentType
  };
  return configs;
}

export const basePath = '';

export interface IList<T> extends Array<T> {}
export interface List<T> extends Array<T> {}
export interface IDictionary<TValue> {
  [key: string]: TValue;
}
export interface Dictionary<TValue> extends IDictionary<TValue> {}

export interface IListResult<T> {
  items?: T[];
}

export class ListResultDto<T> implements IListResult<T> {
  items?: T[];
}

export interface IPagedResult<T> extends IListResult<T> {
  totalCount?: number;
  items?: T[];
}

export class PagedResultDto<T = any> implements IPagedResult<T> {
  totalCount?: number;
  items?: T[];
}

// customer definition
// empty

export class FileService {
  /**
   * Generate Upload Url
   */
  static generateUploadUrlApiV1FileGenerateUploadUrlPost(
    params: {
      /** requestBody */
      body?: FileInfo;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<PreSignedUrl> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/file/generate_upload_url';

      const configs: IRequestConfig = getConfigs('post', 'application/json', url, options);

      let data = params.body;

      configs.data = data;

      axios(configs, resolve, reject);
    });
  }
  /**
   * Download
   */
  static downloadApiV1FileDownloadPost(
    params: {
      /**  */
      filePath: string;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<any | null> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/file/download';

      const configs: IRequestConfig = getConfigs('post', 'application/json', url, options);
      configs.params = { file_path: params['filePath'] };

      axios(configs, resolve, reject);
    });
  }
}

export class ModelFilesService {
  /**
   * Get All
   */
  static getAllApiV1ModelFilesGet(options: IRequestOptions = {}): Promise<ModelFileOut[]> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/model_files/';

      const configs: IRequestConfig = getConfigs('get', 'application/json', url, options);

      axios(configs, resolve, reject);
    });
  }
  /**
   * Create Model File
   */
  static createModelFileApiV1ModelFilesPost(
    params: {
      /** requestBody */
      body?: ModelFileCreateIn;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<ModelFileOut> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/model_files/';

      const configs: IRequestConfig = getConfigs('post', 'application/json', url, options);

      let data = params.body;

      configs.data = data;

      axios(configs, resolve, reject);
    });
  }
  /**
   * Get Model File
   */
  static getModelFileApiV1ModelFilesIdGet(
    params: {
      /**  */
      id: string;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<ModelFileOut> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/model_files/{id}';
      url = url.replace('{id}', params['id'] + '');

      const configs: IRequestConfig = getConfigs('get', 'application/json', url, options);

      axios(configs, resolve, reject);
    });
  }
  /**
   * Delete Model File
   */
  static deleteModelFileApiV1ModelFilesIdDelete(
    params: {
      /**  */
      id: string;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<any | null> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/model_files/{id}';
      url = url.replace('{id}', params['id'] + '');

      const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options);

      axios(configs, resolve, reject);
    });
  }
  /**
   * Update Markers
   */
  static updateMarkersApiV1ModelFilesIdMarkersPut(
    params: {
      /**  */
      id: string;
      /** requestBody */
      body?: UpdateMarkersIn;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<any | null> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/model_files/{id}/markers';
      url = url.replace('{id}', params['id'] + '');

      const configs: IRequestConfig = getConfigs('put', 'application/json', url, options);

      let data = params.body;

      configs.data = data;

      axios(configs, resolve, reject);
    });
  }
}

export class PatrolService {
  /**
   * Get History
   */
  static getHistoryApiViPatrolGetAllGet(
    params: {
      /**  */
      modelFileId?: string;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<PatrolOut[]> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/vi/patrol/get_all';

      const configs: IRequestConfig = getConfigs('get', 'application/json', url, options);
      configs.params = { model_file_id: params['modelFileId'] };

      axios(configs, resolve, reject);
    });
  }
}

export class IssuesService {
  /**
   * Get All Issues
   */
  static getAllIssuesApiV1IssuesGet(options: IRequestOptions = {}): Promise<IssueOut[]> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/issues/';

      const configs: IRequestConfig = getConfigs('get', 'application/json', url, options);

      axios(configs, resolve, reject);
    });
  }
  /**
   * Create Issue
   */
  static createIssueApiV1IssuesPost(
    params: {
      /** requestBody */
      body?: IssueCreateIn;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<IssueOut> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/issues/';

      const configs: IRequestConfig = getConfigs('post', 'application/json', url, options);

      let data = params.body;

      configs.data = data;

      axios(configs, resolve, reject);
    });
  }
  /**
   * Get Issue
   */
  static getIssueApiV1IssuesIdGet(
    params: {
      /**  */
      id: string;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<IssueOut> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/issues/{id}';
      url = url.replace('{id}', params['id'] + '');

      const configs: IRequestConfig = getConfigs('get', 'application/json', url, options);

      axios(configs, resolve, reject);
    });
  }
  /**
   * Update Issue
   */
  static updateIssueApiV1IssuesIdPut(
    params: {
      /**  */
      id: string;
      /** requestBody */
      body?: IssueUpdateIn;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<IssueOut> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/issues/{id}';
      url = url.replace('{id}', params['id'] + '');

      const configs: IRequestConfig = getConfigs('put', 'application/json', url, options);

      let data = params.body;

      configs.data = data;

      axios(configs, resolve, reject);
    });
  }
  /**
   * Delete Issue
   */
  static deleteIssueApiV1IssuesIdDelete(
    params: {
      /**  */
      id: string;
    } = {} as any,
    options: IRequestOptions = {}
  ): Promise<any | null> {
    return new Promise((resolve, reject) => {
      let url = basePath + '/api/v1/issues/{id}';
      url = url.replace('{id}', params['id'] + '');

      const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options);

      axios(configs, resolve, reject);
    });
  }
}

/** CameraPose */
export class CameraPose {
  /**  */
  'position': number[];

  /**  */
  'orientation': number[];

  constructor(data: CameraPose = {}) {
    Object.assign(this, data);
  }
}

/** FileInfo */
export class FileInfo {
  /**  */
  'file_name': string;

  constructor(data: FileInfo = {}) {
    Object.assign(this, data);
  }
}

/** HTTPValidationError */
export class HTTPValidationError {
  /**  */
  'detail'?: ValidationError[];

  constructor(data: HTTPValidationError = {}) {
    Object.assign(this, data);
  }
}

/** IssueCreateIn */
export class IssueCreateIn {
  /**  */
  'issue_type': string;

  /**  */
  'description'?: any | null;

  /**  */
  'image_url'?: any | null;

  /**  */
  'camera_pose'?: any | null;

  /**  */
  'position_3d': number[];

  constructor(data: IssueCreateIn = {}) {
    Object.assign(this, data);
  }
}

/** IssueOut */
export class IssueOut {
  /**  */
  'issue_type': string;

  /**  */
  'description'?: any | null;

  /**  */
  'image_url'?: any | null;

  /**  */
  'camera_pose'?: any | null;

  /**  */
  'position_3d': number[];

  /**  */
  'create_time': Date;

  /**  */
  'update_time'?: any | null;

  /**  */
  'id': string;

  constructor(data: IssueOut = {}) {
    Object.assign(this, data);
  }
}

/** IssueUpdateIn */
export class IssueUpdateIn {
  /**  */
  'issue_type'?: any | null;

  /**  */
  'description'?: any | null;

  /**  */
  'image_url'?: any | null;

  /**  */
  'camera_pose'?: any | null;

  /**  */
  'position_3d'?: any | null;

  constructor(data: IssueUpdateIn = {}) {
    Object.assign(this, data);
  }
}

/** Marker */
export class Marker {
  /**  */
  'id': string;

  /**  */
  'position': number[];

  /**  */
  'orientation': number[];

  constructor(data: Marker = {}) {
    Object.assign(this, data);
  }
}

/** ModelFileCreateIn */
export class ModelFileCreateIn {
  /**  */
  'file_url': string;

  constructor(data: ModelFileCreateIn = {}) {
    Object.assign(this, data);
  }
}

/** ModelFileOut */
export class ModelFileOut {
  /**  */
  'file_url': string;

  /**  */
  'markers'?: Marker[];

  /**  */
  'create_time': Date;

  /**  */
  'id': string;

  constructor(data: ModelFileOut = {}) {
    Object.assign(this, data);
  }
}

/** PatrolOut */
export class PatrolOut {
  /**  */
  'model_file_id': string;

  /**  */
  'start_time': Date;

  /**  */
  'path': PatrolUpdate[];

  /**  */
  'id': string;

  constructor(data: PatrolOut = {}) {
    Object.assign(this, data);
  }
}

/** PatrolUpdate */
export class PatrolUpdate {
  /**  */
  'patrol_id': string;

  /**  */
  'position': Position;

  /**  */
  'orientation': number;

  constructor(data: PatrolUpdate = {}) {
    Object.assign(this, data);
  }
}

/** Position */
export class Position {
  /**  */
  'x': number;

  /**  */
  'y': number;

  /**  */
  'z': number;

  constructor(data: Position = {}) {
    Object.assign(this, data);
  }
}

/** PreSignedUrl */
export class PreSignedUrl {
  /**  */
  'upload_url': string;

  /**  */
  'download_url': string;

  constructor(data: PreSignedUrl = {}) {
    Object.assign(this, data);
  }
}

/** UpdateMarkersIn */
export class UpdateMarkersIn {
  /**  */
  'markers'?: Marker[];

  constructor(data: UpdateMarkersIn = {}) {
    Object.assign(this, data);
  }
}

/** ValidationError */
export class ValidationError {
  /**  */
  'loc': any | null[];

  /**  */
  'msg': string;

  /**  */
  'type': string;

  constructor(data: ValidationError = {}) {
    Object.assign(this, data);
  }
}
