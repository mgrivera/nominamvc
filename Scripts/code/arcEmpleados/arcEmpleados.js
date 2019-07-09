"use strict";

var app = angular.module('myApp', ['ui.bootstrap', 'angularUtils.directives.dirPagination']);

// -------------------------------------------------------------------------------------
// factory para leer (http) datos 'iniciales' del programa; en particular, leemos la cia 
// contab seleccionada por el usuario y una lista de años, para que el usuario seleccione 
// uno cuando quiera consultar los arcs de los empleados ... 
app.factory("DatosInicialesFactory", function ($http, $q) {

    var factory = {};

    factory.listaAnos = [];
    factory.ciaContabSeleccionada = {};

    factory.leerDatosIniciales = function () {

        var deferred = $q.defer();

        var uri = "api/ArcEmpleadosWebApi/LeerDatosIniciales";

        $http.get(uri).
            success(function (data, status) {

                if (data.errorFlag) {
                    deferred.reject({ number: 0, message: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage });
                }
                else {
                    factory.ciaContabSeleccionada.numero = data.ciaContabSeleccionada.numero;
                    factory.ciaContabSeleccionada.nombre = data.ciaContabSeleccionada.nombre;
                    factory.listaAnos.length = 0;

                    _.sortBy(data.listaAnos, function(item) { return item; }).forEach(function (item) {
                        factory.listaAnos.push(item); 
                    });

                    var resolve = {};
                    resolve.listaAnos = factory.listaAnos;
                    resolve.ciaContabSeleccionada = factory.ciaContabSeleccionada;

                    deferred.resolve(resolve);
                }
            }).
            error(function (data, status, headers, config) {
                deferred.reject({ number: status, message: "Error al intentar ejecutar http al servidor." });
            });

        return deferred.promise;
    };

    return factory; 
}); 

app.controller("MainController", function ($scope, $modal, DatosInicialesFactory, ArcEmpleadosFactory) {

    // ui-bootstrap alerts ... 
    $scope.alerts = [];

    // pagination 
    $scope.currentPage = 1;
    $scope.pageSize = 10;

    $scope.closeAlert = function (index) {
        $scope.alerts.splice(index, 1);
    };

    $scope.showProgress = true;

    $scope.arcEmpleados = [];
    $scope.listaResumenesArc = [];              // para 'reducir' arcEmpleados y mostrar en la página ... 
    $scope.arcEmpleados_Totales = {};           // para calcular y registrar los totales generales de la lista y mostrarlos al final de la tabla

    DatosInicialesFactory.leerDatosIniciales().then(
            function resolve(resolve) {
                $scope.listaAnos = _.sortBy(resolve.listaAnos, function(item) { return item * -1 });
                $scope.ciaContabSeleccionada = resolve.ciaContabSeleccionada;

                $scope.showProgress = false;
            },
            function reject(err) {
                $scope.alerts.length = 0;
                $scope.alerts.push({
                    type: 'danger',
                    msg: err.message
                });

                $scope.showProgress = false;
            });



    // -----------------------------------------------------
    // para abrir el modal que permitirá construir los arcs 
    $scope.openConstruirArcsModal = function (size) {

        var modalInstance = $modal.open({
            templateUrl: 'construirArcEmpleadosModal.html',
            controller: 'ConstruirArcsEmpleadosModalController',
            size: size
            //resolve: {
            //    filtro: function () {
            //        return $scope.filtro;
            //    }
            //}
        });

        modalInstance.result.then(
            function (modalReturnValue) {

                var ano = modalReturnValue.ano;
                let agregarMontoUtilidades = modalReturnValue.agregarMontoUtilidades; 

               $scope.showProgress = true;

               // TODO: pasar el valor del checkBox (agregarMontoUtilidades) al factory que viene ...
               var promise = ArcEmpleadosFactory.construirArcsEmpleados(ano, agregarMontoUtilidades, $scope.ciaContabSeleccionada.numero);

               promise.then(
                   function resolve(resolve) {

                       let message = ""; 

                       if (resolve.agregarMontoUtilidades) {
                           message = "Ok, los arcs de empleados han sido construídos para el año indicado.<br />" +
                               resolve.cantidadArcEmpleadosAgregados.toString() + " arcs de empleados fueron construídos (agregados); <br />" +
                               resolve.cantidadArcEmpleadosEliminados.toString() + " arcs de empleados fueron eliminados, pues existían previamente y fueron sustituídos.<br /><br />" +
                               "Los montos de utilidades (" + resolve.cantidadMontosUtilidades.toString() + ") <b>fueron</b> agregados al salario de los empleados.";  
                       } else {
                           message = "Ok, los arcs de empleados han sido construídos para el año indicado.<br />" +
                               resolve.cantidadArcEmpleadosAgregados.toString() + " arcs de empleados fueron construídos (agregados); <br />" +
                               resolve.cantidadArcEmpleadosEliminados.toString() + " arcs de empleados fueron eliminados, pues existían previamente y fueron sustituídos.<br /><br />" +
                               "Los montos de utilidades (" + resolve.cantidadMontosUtilidades.toString() + ") <b>no fueron</b> agregados al salario de los empleados."; 
                       } 
                       

                       $scope.alerts.length = 0;
                       $scope.alerts.push({
                           type: 'info',
                           msg: message
                       }); agregarMontoUtilidades

                       // agregamos el 'ano' que se acaba de construir, a la lista de años disponibles para consultar; de otra forma, el usuario debe 
                       // reiniciar la página para que este año se agregue a la lista ... 

                       if (!_.find($scope.listaAnos, function (item) { return item === ano; }))
                           // nótese como agregamos el año recién construído al inicio de la lista y no al final ... 
                           $scope.listaAnos.unshift(ano); 

                       $scope.showProgress = false; 
                   },
                   function reject(err) {

                       $scope.alerts.length = 0;
                       $scope.alerts.push({
                           type: 'danger',
                           msg: err.message
                       });

                       $scope.showProgress = false;
                   }
                ); 

           }, function (modalCancelValue) {
               // el usuario canceló el modal ... 
               var a = modalCancelValue;

           });
    };


    // -----------------------------------------------------
    // para abrir el modal que permite consultar los arcs  
    $scope.openConsultarArcsModal = function (size) {

        var modalInstance = $modal.open({
            templateUrl: 'consultarArcEmpleadosModal.html',
            controller: 'ConsultarArcsEmpleadosModalController',
            size: size,
            resolve: {
                listaAnos: function () {
                    return $scope.listaAnos;
                }
            }
        });

        modalInstance.result.then(
           function (modalReturnValue) {
               // regresamos el año desde el modal 
               var ano = modalReturnValue;

               $scope.showProgress = true;

               var promise = ArcEmpleadosFactory.consultarArcsEmpleados(ano, $scope.ciaContabSeleccionada.numero);

               promise.then(
                   function resolve(resolve) {

                       $scope.arcEmpleados = ArcEmpleadosFactory.arcEmpleados;

                       // 'reducimos' el array de empleados y sus arcs, para mostrarlo comodamente en una tabla; 
                       // los valores para cada mes vienen en un array (de hasta 12 meses); producimos un array 
                       // muy simple; cada linea contiene un empleado y sus totales para remuneración, islr y sso 

                       $scope.listaResumenesArc.length = 0;

                       $scope.arcEmpleados.forEach(function (item) {

                           var resumen = {};

                           resumen.id = item.Id;
                           resumen.ano = item.Ano;
                           resumen.nombre = item.Nombre;
                           resumen.cantMeses = item.MontosMensuales.length;

                           // usamos reduce para obtener un sum de los valores de cada item en el array 

                           resumen.remuneracion = item.MontosMensuales.reduce(function (total, curr) { return total + curr.Remuneracion }, 0);
                           resumen.sso = item.MontosMensuales.reduce(function (total, curr) { return total + curr.Sso }, 0);
                           resumen.islr = item.MontosMensuales.reduce(function (total, curr) { return total + curr.Islr }, 0);

                           $scope.listaResumenesArc.push(resumen);
                       });

                       // finalmente, calculamos los totales generales para toda la lita, para mostrarlos como footer en la tabla 

                       $scope.arcEmpleados_Totales.remuneracion = $scope.listaResumenesArc.reduce(function (total, curr) { return total + curr.remuneracion }, 0);
                       $scope.arcEmpleados_Totales.sso = $scope.listaResumenesArc.reduce(function (total, curr) { return total + curr.sso }, 0);
                       $scope.arcEmpleados_Totales.islr = $scope.listaResumenesArc.reduce(function (total, curr) { return total + curr.islr }, 0);

                       var message = "Ok, " + $scope.arcEmpleados.length.toString() + " empleados " +
                                     " han sido leídos desde la base de datos, para el año " +
                                     ano.toString() +
                                     ".";

                       $scope.alerts.length = 0;
                       $scope.alerts.push({
                           type: 'info',
                           msg: message
                       });

                       $scope.showProgress = false;
                   },
                   function reject(err) {

                       $scope.alerts.length = 0;
                       $scope.alerts.push({
                           type: 'danger',
                           msg: err.message
                       });

                       $scope.showProgress = false;
                   }
                );

           }, function (modalCancelValue) {
               // el usuario canceló el modal ... 
               var a = modalCancelValue;

           });
    };


    // ------------------------------------------------------
    // para consultar los detalles del arc de un empleado 

    $scope.openMostrarDetallesArcEmpleadoModal = function (size, empleadoID) {

        var modalInstance = $modal.open({
            templateUrl: 'consultarDetallesArcEmpleadosModal.html',
            controller: 'ConsultarDetallesArcsEmpleadosModalController',
            size: size,
            resolve: {
                arcEmpleadoSeleccionado: function () {
                    var arcEmpleado = _.find($scope.arcEmpleados, function(item) { return item.Id == empleadoID });
                    return arcEmpleado; 
                }
            }
        });

        modalInstance.result.then(
           function (modalReturnValue) {
               // regresamos el año desde el modal 
               var a = modalReturnValue;
           }, function (modalCancelValue) {
               // el usuario canceló el modal ... 
               var a = modalCancelValue;

           });
    };

    $scope.openConvertirWordModal = function (size) {

        var modalInstance = $modal.open({
            templateUrl: 'convertirWordModal.html',
            controller: 'ConvertirWordModalController',
            size: size,
            resolve: {
                arcEmpleados_lista: function () {
                    // pasamos la lista de arcs de empleados al controller ... 
                    return $scope.arcEmpleados;
                }
            }
        });

        modalInstance.result.then(
           function (modalReturnValue) {
               // regresamos el año desde el modal 
               var a = modalReturnValue;
           }, function (modalCancelValue) {
               // el usuario canceló el modal ... 
               var a = modalCancelValue;
           });
    }; 
});


// ----------------------------------------------------------------------------------------------------------
// controller para el modal que permite consultar los arcs de empleados 
app.controller("ConsultarArcsEmpleadosModalController", function ($scope, $modalInstance, listaAnos) {

    $scope.alerts = [];
    $scope.listaAnos = listaAnos

    $scope.closeAlert = function (index) {
        $scope.alerts.splice(index, 1);
    };

    $scope.ok = function () {
        $modalInstance.close($scope.ano);
    };

    $scope.consultarArcsModalForm_Submit = function () {

        // dificil aplicar el required cuando usamos una lista en vez de un combo (en el Select) 
        // por eso validamos aquí ... 

        // nótese que el Select siempre regresa un array, aunque el usuario seleccione un solo valor ... 

        if (!$scope.ano || !_.isArray($scope.ano) || (_.isArray($scope.ano) && $scope.ano.length != 1)) 
            $scope.consultarArcsModalForm.$valid = false; 

        if (!$scope.consultarArcsModalForm.$valid) {
            // intentamos agregar alguna validación a la forma 

            $scope.alerts.length = 0;
            $scope.alerts.push({
                type: 'danger',
                msg: "Ud. debe seleccionar un año de la lista."
            });

            return;
        }
        else
            $modalInstance.close($scope.ano[0]); ;
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('the modal was canceled');
    };

}); 


// ---------------------------------------------------------------------------------------------------------
// controller para el modal que permite construir los arcs de empleados 
app.controller("ConstruirArcsEmpleadosModalController", function ($scope, $modalInstance) {

    // $scope.ano = new Number();
    $scope.alerts = [];

    $scope.closeAlert = function (index) {
        $scope.alerts.splice(index, 1);
    };

    $scope.ok = function () {
        let returnValue = {
            ano: $scope.ano,
            agregarMontoUtilidades: $scope.agregarMontoUtilidades ? $scope.agregarMontoUtilidades : false
        }; 

        $modalInstance.close(returnValue);
    };

    $scope.agregarArcsModalForm_Submit = function () {

        if (!$scope.agregarArcsModalForm.$valid) {
            // intentamos agregar alguna validación a la forma 

            $scope.alerts.length = 0;
            $scope.alerts.push({
                type: 'danger',
                msg: "Aparentemente, el valor indicado para el campo año no es válido. " +
                     "Ud. debe indicar un valor adecuado para este campo; ejemplo: 2013, 2014, etc."
            });

            return;
        }
        else {
            let returnValue = {
                ano: $scope.ano,
                agregarMontoUtilidades: $scope.agregarMontoUtilidades ? $scope.agregarMontoUtilidades : false
            };

            $modalInstance.close(returnValue);
        }
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('the modal was canceled');
    };
});


// ---------------------------------------------------------------------------------------------------------
// controller para el modal que permite construir los arcs de empleados 
app.controller("ConsultarDetallesArcsEmpleadosModalController", function ($scope, $modalInstance, arcEmpleadoSeleccionado) {

    // $scope.ano = new Number();
    $scope.alerts = [];
    $scope.arcEmpleadoSeleccionado = arcEmpleadoSeleccionado; 
    $scope.arcEmpleadoSeleccionado_totales = {}; 

    // calculamos los totales para el empleado seleccionado, para mostrarlos en la tabla de montos mensuales para el empleado 
    $scope.arcEmpleadoSeleccionado_totales.remuneracion = arcEmpleadoSeleccionado.MontosMensuales.reduce( 
                                                          function(sum, curr) { return sum + curr.Remuneracion; }, 0); 

    $scope.arcEmpleadoSeleccionado_totales.sso = arcEmpleadoSeleccionado.MontosMensuales.reduce( 
                                                          function(sum, curr) { return sum + curr.Sso; }, 0); 

    $scope.arcEmpleadoSeleccionado_totales.islr = arcEmpleadoSeleccionado.MontosMensuales.reduce( 
                                                          function(sum, curr) { return sum + curr.Islr; }, 0); 


    $scope.closeAlert = function (index) {
        $scope.alerts.splice(index, 1);
    };

    $scope.ok = function () {
        $modalInstance.close($scope.ano);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('the modal was canceled');
    };

    // ----------------------------------------------------------------------
    // para regresar el nombre del mes y mostrarlo al usuario en la 
    // tabla de detalles para el empleado seleccionado 
    $scope.nombreMes = function(mes) { 
        switch (mes) 
        {
            case 1: 
                return "Enero"; 
                break; 
            case 2: 
                return "Febrero"; 
                break; 
            case 3: 
                return "Marzo"; 
                break; 
            case 4: 
                return "Abril"; 
                break;  
            case 5: 
                return "Mayo"; 
                break;  
            case 6: 
                return "Junio"; 
                break; 
            case 7: 
                return "Julio"; 
                break; 
            case 8: 
                return "Agosto"; 
                break; 
            case 9: 
                return "Septiembre"; 
                break; 
            case 10: 
                return "Octubre"; 
                break;
            case 11: 
                return "Noviembre"; 
                break; 
            case 12: 
                return "Diciembre"; 
                break; 
            default: 
                return "Indefinido"; 
                break; 
        }; 
    }; 
});

// -------------------------------------------------------------------------------------------------------
// controller para abrir un modal que permite al usuario convertir a un documento Excel 
app.controller("ConvertirWordModalController", function ($scope, $modalInstance, $http, arcEmpleados_lista) {

    $scope.alerts = [];
    $scope.listaPlantillasWord = []; 
    $scope.plantillaWord = ""; 
    $scope.showProgress = false; 

    // para obtener una lista de las plantillas Word (doc y docx) que permiten obtener este tipo de documentos 
    // (arcs de empleados) 
    $scope.obtenerListaPlantillasWord = function () {

        var uri = "api/ArcEmpleadosWebApi/GetListaPlantillasWord";
        $scope.showProgress = true; 

        $http.get(uri).
            success(function (data, status) {

                if (data.errorFlag) {
                    $scope.alerts.length = 0;
                    $scope.alerts.push({
                        type: 'danger',
                        msg: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage
                    });

                    $scope.showProgress = false;
                }
                else {

                    // recibimos del servidor una lista con los nombres de documentos Word que existan en el 
                    // directorio './Word/ArcEmpleados' 

                    $scope.listaPlantillasWord.length = 0;
                    _.sortBy(data.plantillasWord, function(item) { 
                        return item; 
                    }).forEach(function (item) {
                        $scope.listaPlantillasWord.push(item);
                    });

                    $scope.showProgress = false;
                }
            }).
            error(function (data, status, headers, config) {
                $scope.alerts.length = 0;
                $scope.alerts.push({
                    type: 'danger',
                    msg: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage
                });

                $scope.showProgress = false;
            });
    }; 

    $scope.obtenerListaPlantillasWord(); 

    $scope.construirDocumentoWord = function (plantillaWord) {

        $scope.showProgress = true; 
        var uri = "api/ArcEmpleadosWebApi/ConvertirWord?plantillaWord=" + plantillaWord; 

        $http.post(uri, arcEmpleados_lista).
            success(function (data, status) {

                if (data.errorFlag) {
                    $scope.alerts.length = 0;
                    $scope.alerts.push({
                        type: 'danger',
                        msg: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage
                    });

                    $scope.showProgress = false;
                }
                else {
                    $scope.alerts.length = 0;
                    $scope.alerts.push({
                        type: 'info',
                        msg: "Ok, el documento Word se ha construído en forma satisfactoria. Por favor haga un click en " + 
                             "<em><b>Download</b></em> para obtener  una copia del mismo. "
                    });

                    $scope.showProgress = false;
                }
            }).
            error(function (data, status, headers, config) {
                $scope.alerts.length = 0;
                $scope.alerts.push({
                    type: 'danger',
                    msg: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage
                });

                $scope.showProgress = false;
            });
    };

     $scope.convertirWordForm_Submit = function () {

        if (!$scope.convertirWordForm.$valid) {
            // intentamos agregar alguna validación a la forma 

            $scope.alerts.length = 0;
            $scope.alerts.push({
                type: 'danger',
                msg: "Aparentemente, Ud. no ha seleccionado una plantilla (Word) de la lista. <br />" +
                     "Debe seleccionar una plantilla Word, antes de intentar construir los documentos."
            });

            return;
        }
        else
            $scope.construirDocumentoWord($scope.plantillaWord);
    };


    $scope.downloadDocumentoWord = function () {

        $scope.showProgress = true;
        var uri = "api/ArcEmpleadosWebApi/DownloadDocumentoWord?plantillaWord=" + $scope.plantillaWord;

        $http.get(uri).
            success(function (data, status) {

                if (data.errorFlag) {
                    $scope.alerts.length = 0;
                    $scope.alerts.push({
                        type: 'danger',
                        msg: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage
                    });

                    $scope.showProgress = false;
                }
                else {
                    //$scope.alerts.length = 0;
                    //$scope.alerts.push({
                    //    type: 'info',
                    //    msg: "Ok, el documento Word se ha construído en forma satisfactoria. Por favor haga un click en " +
                    //         "<em><b>Download</b></em> para obtener  una copia del mismo. "
                    //});

                    $scope.showProgress = false;
                }
            }).
            error(function (data, status, headers, config) {
                $scope.alerts.length = 0;
                $scope.alerts.push({
                    type: 'danger',
                    msg: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage
                });

                $scope.showProgress = false;
            });

    }; 
    
    $scope.closeAlert = function (index) {
        $scope.alerts.splice(index, 1);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('the modal was canceled');
    };
}); 


app.factory("ArcEmpleadosFactory", function($http, $q) { 

    var factory = {}; 

    factory.arcEmpleados = []; 

    factory.construirArcsEmpleados = function (ano, agregarMontoUtilidades, ciaContabSeleccionada) {

        var deferred = $q.defer();

        var uri = "api/ArcEmpleadosWebApi/ConstruirArcEmpleadosParaUnAno?ano=" + ano.toString() + "&agregarMontoUtilidades=" + agregarMontoUtilidades.toString() + "&ciaContabSeleccionada=" + ciaContabSeleccionada.toString();

        $http.post(uri).
            success(function (data, status) {

                if (data.errorFlag) {
                    deferred.reject({ number: 0, message: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage });
                }
                else {
                    var resolve = {};
                    resolve.cantidadArcEmpleadosAgregados = data.cantidadArcEmpleadosAgregados;
                    resolve.cantidadArcEmpleadosEliminados = data.cantidadArcEmpleadosEliminados;
                    resolve.agregarMontoUtilidades = data.agregarMontoUtilidades;
                    resolve.cantidadMontosUtilidades = data.cantidadMontosUtilidades ? data.cantidadMontosUtilidades : 0; 

                    deferred.resolve(resolve);
                }
            }).
            error(function (data, status, headers, config) {
                deferred.reject({ number: status, message: "Error al intentar ejecutar http al servidor." });
            });

        return deferred.promise;

    };


    factory.consultarArcsEmpleados = function (ano, ciaContabSeleccionada) {

        var deferred = $q.defer();

        var uri = "api/ArcEmpleadosWebApi/ConsultarArcEmpleadosParaUnAno?ano=" + ano.toString() + "&ciaContabSeleccionada=" + ciaContabSeleccionada.toString();

        $http.get(uri).
            success(function (data, status) {

                if (data.errorFlag) {
                    deferred.reject({ number: 0, message: "Error al intentar ejecutar http al servidor.<br />" + data.resultMessage });
                }
                else {
                    factory.arcEmpleados.length = 0;

                    data.arcEmpleados.forEach(function (item) {
                        factory.arcEmpleados.push(item); 
                    }); 

                    deferred.resolve(factory.arcEmpleados.length);
                }
            }).
            error(function (data, status, headers, config) {
                deferred.reject({ number: status, message: "Error al intentar ejecutar http al servidor." });
            });

        return deferred.promise;

    };

    return factory; 
}); 



// ---------------------------------------------------------------------------------------
// para mostrar 'unsafe' strings (with embedded html) in ui-bootstrap alerts .... 
// ---------------------------------------------------------------------------------------
app.filter('unsafe', function ($sce) {
    return function (value) {
        if (!value) { return ''; }
        return $sce.trustAsHtml(value);
    };
});


