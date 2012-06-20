/*
	FizzBuzz in GO, as a webserver...
*/
package main

import (
	"flag"
	"fmt"
	"net/http"
	c "strconv"
	"text/template"
)

var num int
var index = template.Must(template.ParseFiles("index.html"))
var fresh = template.Must(template.ParseFiles("fizzbuzz.html"))

func main() {

	flag.IntVar(&num, "index", 0, "numeric index")
	host := flag.String("host", ":8008", "host to bind to")

	flag.Parse()

	http.HandleFunc("/", indexHandler)
	http.HandleFunc("/fizzbuzz", fizzbuzzHandler)

	fmt.Printf("listening on %s with index %d\r\n", *host, num)
	http.ListenAndServe(*host, nil)

}

func indexHandler(res http.ResponseWriter, req *http.Request) {

	err := index.Execute(res, num)
	if err != nil {
		res.Write([]byte("<big>BALLS!</big>"))
	}
}

func fizzbuzzHandler(res http.ResponseWriter, req *http.Request) {
	num++
	err := fresh.Execute(res, fizzbuzz(num))
	if err != nil {
		res.Write([]byte("<i>SUCK!/i>"))
	}
}

func fizzbuzz(hc int) (g string) {

	if hc%5 == 0 && hc%3 == 0 {
		return ("FizzBuzz")
	}
	if hc%5 == 0 {
		return ("Buzz")
	}
	if hc%3 == 0 {
		return ("Fizz")
	}
	
	// int to string?
	// 'dogs and cats living together...'
	return c.Itoa(hc)
}
