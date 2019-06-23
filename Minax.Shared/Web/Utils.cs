﻿using Minax.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minax.Web
{
	public static class Utils
	{
		public static async Task<bool> FetchFilesByFileListLink( string fileListLink, string targetPath,
											OverwritePolicy policy, CancellationTokenSource cancelToken,
											IProgress<ProgressInfo> progress )
		{
			if( string.IsNullOrWhiteSpace( fileListLink ) ||
				string.IsNullOrWhiteSpace( targetPath ) ||
				policy == OverwritePolicy.AlwaysAsking ||
				policy == OverwritePolicy.FileDateNew )
				return false;

			try {
				Uri uri = new Uri( fileListLink );

				if( Directory.Exists( targetPath ) == false )
					Directory.CreateDirectory( targetPath );

				// fetch by FTP protocol
				if( uri.Scheme == Uri.UriSchemeFtp ) {
					return await FetchFilesByFileListLink( uri, targetPath, policy, cancelToken, progress );
				}

				using( var client = new HttpClient() ) {
					var response = await client.GetAsync( uri );
					if( response == null || response.IsSuccessStatusCode == false ) {
						progress?.Report( new ProgressInfo {
							PercentOrErrorCode = -1,
							Message = $"Cannot fetch remote glossary list file.",
							InfoObject = response,
						} );
						return false;
					}

					var responseString = await response.Content.ReadAsStringAsync();
					if( string.IsNullOrWhiteSpace( responseString ) ) {
						progress?.Report( new ProgressInfo {
							PercentOrErrorCode = 100,
							Message = $"No glossary file can fetch.",
						} );
						return true;
					}

					var remoteRelFiles = responseString.Split( new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries );
					if( remoteRelFiles == null || remoteRelFiles.Length <= 0 ) {
						progress?.Report( new ProgressInfo {
							PercentOrErrorCode = 100,
							Message = $"No glossary file can fetch.",
						} );
						return true;
					}

					for( int i = 0; i < remoteRelFiles.Length; ++i ) {
						var relFn = remoteRelFiles[i];
						try {
							var locFn = Path.GetFullPath( Path.Combine( targetPath, relFn ) );
							if( Directory.Exists( locFn ) ) {
								continue;
							}

							Uri relUri = new Uri( uri, relFn );

							response = await client.GetAsync( relUri );
							if( response == null || response.IsSuccessStatusCode == false ||
								response.Content.Headers.ContentLength <= 0 )
								continue;

							if( File.Exists( locFn ) ) {
								switch( policy ) {
									case OverwritePolicy.Skip:
										continue;
									case OverwritePolicy.FileSizeLarger:
										using( var fileStream = new FileStream( locFn, FileMode.Open, FileAccess.Read, FileShare.Read ) ) {
											if( fileStream.Length >= response.Content.Headers.ContentLength )
												continue;
										}
										break;
								}
							}

							using( var stream = new FileStream( locFn, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) ) {
								await response.Content.CopyToAsync( stream );
							}

							progress?.Report( new ProgressInfo {
								PercentOrErrorCode = (i + 1) / remoteRelFiles.Length,
								Message = $"Fetched {i + 1}/{remoteRelFiles.Length} files.",
							} );
						}
						catch { }
					}
				}
			}
			catch( Exception ex ) {
				progress?.Report( new ProgressInfo {
					PercentOrErrorCode = -1,
					Message = ex.Message,
					InfoObject = ex
				} );
				return false;
			}

			progress?.Report( new ProgressInfo {
				PercentOrErrorCode = 100,
				Message = $"All glossary files fetched.",
			} );

			return true;
		}


		public static async Task<bool> FetchFilesByFileListLink( Uri ftpFileListLink, string targetPath,
											OverwritePolicy policy, CancellationTokenSource cancelToken,
											IProgress<ProgressInfo> progress )
		{
			if( ftpFileListLink == null || ftpFileListLink.Scheme != Uri.UriSchemeFtp ||
				string.IsNullOrWhiteSpace( targetPath ) ||
				policy == OverwritePolicy.AlwaysAsking )
				return false;

			try {
				if( Directory.Exists( targetPath ) == false )
					Directory.CreateDirectory( targetPath );

				FtpWebRequest request = (FtpWebRequest)WebRequest.Create( ftpFileListLink );
				request.Method = WebRequestMethods.Ftp.DownloadFile;
				request.Timeout = 5000;

				FtpWebResponse response = await request.GetResponseAsync() as FtpWebResponse;
				// https://docs.microsoft.com/en-us/dotnet/api/system.net.ftpwebresponse.statuscode
				if( response == null || response.ContentLength <= 0 ||
					response.StatusCode != FtpStatusCode.CommandOK ) {
					progress?.Report( new ProgressInfo {
						PercentOrErrorCode = 100,
						Message = $"Cannot fetch remote glossary list file.",
						InfoObject = response,
					} );
					return false;
				}

				string responseString = null;
				var stream = response.GetResponseStream();
				using( var reader = new StreamReader( stream, Encoding.UTF8 ) ) {
					responseString = await reader.ReadToEndAsync();
				}
				response.Close();

				if( string.IsNullOrWhiteSpace( responseString ) ) {
					progress?.Report( new ProgressInfo {
						PercentOrErrorCode = 100,
						Message = $"No glossary file can fetch.",
					} );
					return false;
				}

				var remoteRelFiles = responseString.Split( new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries );
				if( remoteRelFiles == null || remoteRelFiles.Length <= 0 ) {
					progress?.Report( new ProgressInfo {
						PercentOrErrorCode = 100,
						Message = $"No glossary file can fetch.",
					} );
					return true;
				}

				for( int i = 0; i < remoteRelFiles.Length; ++i ) {
					var relFn = remoteRelFiles[i];
					try {
						var locFn = Path.GetFullPath( Path.Combine( targetPath, relFn ) );
						if( Directory.Exists( locFn ) ) {
							continue;
						}

						Uri relUri = new Uri( ftpFileListLink, relFn );
						request = (FtpWebRequest)WebRequest.Create( relUri );
						request.Method = WebRequestMethods.Ftp.DownloadFile;
						request.Timeout = 5000;

						response = await request.GetResponseAsync() as FtpWebResponse;
						if( response == null ||	response.ContentLength <= 0 ||
							response.StatusCode != FtpStatusCode.CommandOK )
							continue;

						if( File.Exists( locFn ) ) {
							switch( policy ) {
								case OverwritePolicy.Skip:
									continue;
								case OverwritePolicy.FileSizeLarger:
									using( var fileStream = new FileStream( locFn, FileMode.Open, FileAccess.Read, FileShare.Read ) ) {
										if( fileStream.Length >= response.ContentLength )
											continue;
									}
									break;
								case OverwritePolicy.FileDateNew:
									var reqDate = (FtpWebRequest)WebRequest.Create( relUri );
									reqDate.Method = WebRequestMethods.Ftp.GetDateTimestamp;
									reqDate.Timeout = 3000;
									var rspDate = await reqDate.GetResponseAsync() as FtpWebResponse;
									if( rspDate == null || rspDate.StatusCode != FtpStatusCode.CommandOK )
										continue;
									var locDate = File.GetLastWriteTimeUtc( locFn );
									rspDate.Close();
									if( rspDate.LastModified.ToUniversalTime() < locDate )
										continue;
									break;
							}
						}

						using( var fileStream = new FileStream( locFn, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) ) {
							await response.GetResponseStream().CopyToAsync( fileStream );
							response.Close();
						}

						progress?.Report( new ProgressInfo {
							PercentOrErrorCode = (i + 1) / remoteRelFiles.Length,
							Message = $"Fetched {i + 1}/{remoteRelFiles.Length} files.",
						} );
					}
					catch { }
				}
			}
			catch( Exception ex ) {
				progress?.Report( new ProgressInfo {
					PercentOrErrorCode = -1,
					Message = ex.Message,
					InfoObject = ex
				} );
				return false;
			}

			progress?.Report( new ProgressInfo {
				PercentOrErrorCode = 100,
				Message = $"All glossary files fetched.",
			} );

			return true;
		}

	}
}