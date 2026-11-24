using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Common.Opciones;
using Microsoft.Extensions.Options;
using System.Net;

namespace ClinicSyncServer.Infrastructure.Storage
{
    /// <summary>
    /// Clase que gestiona la subida y descarga de archivos utilizando el servicio
    /// Cloudflare R2.
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Common.Abstracciones.IStorageService" />

    public class R2StorageService(IAmazonS3 s3, IOptions<OpcionesR2> opciones) : IStorageService
    {

        /// <summary>
        /// The s3
        /// </summary>
        private readonly IAmazonS3 _s3 = s3;
        /// <summary>
        /// The bucket
        /// </summary>
        private readonly string _bucket = opciones.Value.Bucket;

        /// <summary>
        /// Subirs the asynchronous.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="contenido">The contenido.</param>
        /// <param name="tipoContenido">The tipo contenido.</param>
        /// <param name="ct">The ct.</param>
        public async Task SubirAsync(string clave, Stream contenido, 
            string tipoContenido, CancellationToken ct = default)
        {
            await _s3.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = clave,
                    InputStream = contenido,
                    ContentType = tipoContenido,

                    // Cloudflare R2 no soporta el firmado de payloads de Amazon.
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true
                },
                ct
            );
        }


        /// <summary>
        /// Abrirs the asynchronous.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<ContenidoAlmacenado?> AbrirAsync(string clave, CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _s3.GetObjectAsync(_bucket, clave, ct);
                return new ContenidoAlmacenado(respuesta.ResponseStream, respuesta.Headers.ContentType, respuesta);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <summary>
        /// Eliminars the asynchronous.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="ct">The ct.</param>
        public async Task EliminarAsync(string clave, CancellationToken ct = default)
        {
            await _s3.DeleteObjectAsync(_bucket, clave, ct);
        }
    }
}
